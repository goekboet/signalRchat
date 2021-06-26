import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

const connection = 
    new HubConnectionBuilder()
        .withUrl('/chathub')
        .configureLogging(LogLevel.Information)
        .build()

async function Start() {
    try {
        await connection.start()
        console.log('SignalR connected.')
    } catch (err) {
        console.log(err)
        setTimeout(Start, 5000)
    }
}

connection.onclose(Start)

Start()