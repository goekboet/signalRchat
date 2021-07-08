# SignalR chatclient

This project implements a simple browser-based chatclient with SignalR.

## Getting Started

### Dependencies

* You need a redis-server running and listening to the standard port (6379). One simple way is to run the special-purpose windows build. Can be installed from [here](https://riptutorial.com/redis/example/29962/installing-and-running-redis-server-on-windows)
* Check that you have npm `npm -v` or [install it](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm)
* To build testclient you need tsc and rollup on your $PATH `npm install -g typescript` and `npm install -g rollup`
* You need to be able to run bash-script. If on windows git-bash does the job.

### Build and run
1. `cd src/Client`
2. `./build.sh`
3. `cd ../Server`
4. `dotnet run`


## Features

- Cooke authentication (0-security)
- Keep list of all connected users updated via websocket
- Broadcast chatmessage to anybody listening
- send chat messages via named subset of users (i.e. channel/room). 
- Persists channels.
- Persists chatmessages.
- js-client


