module SystemFX.Core.Tests.Program

open System
open SystemFX.Core

let runStreamExample() =
    // Creare un flusso di numeri
    let numbers = [1; 2; 3; 4; 5]
    let stream = Stream.ofSeq numbers
    // Applicare una trasformazione: raddoppiare i numeri
    let doubledStream = Stream.map (fun x -> x * 2) stream
    // Filtrare solo i numeri pari
    let evenStream = Stream.filter (fun x -> x % 2 = 0) doubledStream
    // Elaborare e stampare ogni elemento
    let subscription =
        Stream.iter (fun x ->
            printfn "Elemento processato: %d" x
        ) evenStream
    // Attendere un momento per consentire l'elaborazione
    System.Threading.Thread.Sleep(100)
    // Rilasciare la sottoscrizione
    subscription.Dispose()

let main argv =
    runStreamExample()
    0 // Codice di uscita