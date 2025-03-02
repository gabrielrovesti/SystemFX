namespace SystemFX.Core.Tests

open Xunit
open System
open SystemFX.Core

type StreamTests() =
    [<Fact>]
    member _.``Test Stream Creation``() =
        let numbers = [1; 2; 3; 4; 5]
        let stream = Stream.ofSeq numbers
       
        Assert.NotNull(stream)
        Assert.NotNull(stream.Id)

    [<Fact>]
    member _.``Test Stream Map``() =
        let numbers = [1; 2; 3; 4; 5]
        let stream = Stream.ofSeq numbers
        let mappedStream = Stream.map (fun x -> x * 2) stream
       
        let mutable result = []
        let subscription =
            Stream.iter (fun x -> result <- x :: result) mappedStream
       
        // Attendere un momento per consentire l'elaborazione
        System.Threading.Thread.Sleep(100)
       
        // Rimuovere la sottoscrizione
        subscription.Dispose()
       
        // Invertire e confrontare
        let expected = [10; 8; 6; 4; 2]
        Assert.Equal<int list>(expected, result)

    [<Fact>]
    member _.``Test Stream Filter``() =
        let numbers = [1; 2; 3; 4; 5; 6; 7; 8; 9; 10]
        let stream = Stream.ofSeq numbers
        let filteredStream = Stream.filter (fun x -> x % 2 = 0) stream
       
        let mutable result = []
        let subscription =
            Stream.iter (fun x -> result <- x :: result) filteredStream
       
        // Attendere un momento per consentire l'elaborazione
        System.Threading.Thread.Sleep(100)
       
        // Rimuovere la sottoscrizione
        subscription.Dispose()
       
        // Invertire e confrontare
        let expected = [10; 8; 6; 4; 2]
        Assert.Equal<int list>(expected, result)