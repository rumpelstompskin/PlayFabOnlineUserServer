# PlayFab Friends List Online Status Server

The server's purpose is to keep track of users online status while they play your game.

## Description

This C# tcp server is a hobby project. I wanted to learn about server's and server's architecture. I found a guide which suited my needs and studied it.
I still have much to learn but I believe I am making progress.

PlayFab FriendList does not include any Online status updates. 
While there are solutions such as SignalR, I decided to go with C# TCP server from scratch.

This server acts as a relay station for your clients to allow for small amounts of data transfer.
When your players launches your title, their clients handshake this server, tells it that they are now online.
The server can then be queried by other users for this clients status.
Information such as PlayFabID and PlayFabNetworkID can also be relayed.

## Getting Started

### Dependencies

* Visual Studio
* Unity

### Installing

* Coming Soon

### Executing program

* Coming Soon

## Help

* Coming Soon

## Authors

Rumpelstompskin

https://www.metagamez.net

## Version History

* 0.2
    * Prototyping data exchange functionality
    * See [commit change]() or See [release history]()
* 0.1
    * Initial Release

## License

This project is licensed under the [NAME HERE] License - see the LICENSE.md file for details

## Acknowledgments

* Tom Weiland (https://tomweiland.net/networking-tutorial-server-client-connection/)
