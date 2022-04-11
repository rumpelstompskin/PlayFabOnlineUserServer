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
* 0.3
    * Added UserInfoRequest Packet
    * Added new packet to InitializePacket
    * Added method to receive and return UserInfoRequest packets.
    * Added more comments to code.
* 0.2
    * Prototyping data exchange functionality
* 0.1
    * Initial Release

## License

MIT License

Copyright (c) [year] [fullname]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Acknowledgments

* Tom Weiland (https://tomweiland.net/networking-tutorial-server-client-connection/)