namespace SystemFX.Core

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Collections.Concurrent

/// Rappresenta uno stream di dati tipizzati
type Stream<'T> = 
    { Id: string
      Subscribe: (IObserver<'T> -> IDisposable) }

/// Definisce i core operators per lo stream processing
module Stream =
    /// Crea un nuovo stream da una sequenza di valori
    let ofSeq (items: seq<'T>) : Stream<'T> =
        let id = Guid.NewGuid().ToString()
        
        let subscribe (observer: IObserver<'T>) =
            let mutable completed = false
            let subscription = 
                { new IDisposable with
                    member _.Dispose() = completed <- true }
            
            Task.Run(fun () ->
                try
                    for item in items do
                        if not completed then
                            observer.OnNext(item)
                    
                    if not completed then
                        observer.OnCompleted()
                with
                | ex -> observer.OnError(ex)
            ) |> ignore

            subscription
        
        { Id = id; Subscribe = subscribe }
    
    /// Trasforma ogni elemento dello stream usando la funzione specificata
    let map (f: 'T -> 'U) (stream: Stream<'T>) : Stream<'U> =
        let id = Guid.NewGuid().ToString()
        
        let subscribe (observer: IObserver<'U>) =
            stream.Subscribe(
                { new IObserver<'T> with
                    member _.OnNext(value) = 
                        try
                            let result = f value
                            observer.OnNext(result)
                        with
                        | ex -> observer.OnError(ex)
                    
                    member _.OnError(error) = observer.OnError(error)
                    
                    member _.OnCompleted() = observer.OnCompleted()
                }
            )
        
        { Id = id; Subscribe = subscribe }
    
    /// Filtra gli elementi dello stream secondo il predicato
    let filter (predicate: 'T -> bool) (stream: Stream<'T>) : Stream<'T> =
        let id = Guid.NewGuid().ToString()
        
        let subscribe (observer: IObserver<'T>) =
            stream.Subscribe(
                { new IObserver<'T> with
                    member _.OnNext(value) = 
                        try
                            if predicate value then
                                observer.OnNext(value)
                        with
                        | ex -> observer.OnError(ex)
                    
                    member _.OnError(error) = observer.OnError(error)
                    
                    member _.OnCompleted() = observer.OnCompleted()
                }
            )
        
        { Id = id; Subscribe = subscribe }
    
    /// Esegue una funzione per ogni elemento dello stream
    let iter (action: 'T -> unit) (stream: Stream<'T>) : IDisposable =
        stream.Subscribe(
            { new IObserver<'T> with
                member _.OnNext(value) = 
                    try
                        action value
                    with
                    | _ -> () // Ignora gli errori nell'azione
                
                member _.OnError(_) = ()
                
                member _.OnCompleted() = ()
            }
        )
    
    /// Raccoglie gli elementi dello stream in una collezione
    let collect (stream: Stream<'T>) : Task<'T[]> =
        let tcs = TaskCompletionSource<'T[]>()
        let items = ConcurrentBag<'T>()
        
        let subscription = stream.Subscribe(
            { new IObserver<'T> with
                member _.OnNext(value) = items.Add(value)
                
                member _.OnError(error) = tcs.SetException(error)
                
                member _.OnCompleted() = 
                    tcs.SetResult(items.ToArray())
            }
        )
        
        // Gestisce la cancellazione tramite il task
        tcs.Task.ContinueWith(fun (_: Task<'T[]>) -> subscription.Dispose()) |> ignore
        
        tcs.Task