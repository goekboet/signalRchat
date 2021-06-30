import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { Lobby } from './lobby.js'

export class App extends HTMLElement {
    _connection : HubConnection
    _lobby : Lobby

    connect() {
        this._connection.start().then(() => {
        this._lobby.Connection = this._connection
        }).catch(err => {
            this.connect()
        })
    }

    constructor() {
        super()

        let connection = new HubConnectionBuilder()
            .withUrl('/chathub')
            .configureLogging(LogLevel.Information)
            .build()

        let lobby = new Lobby()

        this._lobby = lobby
        this._connection = connection;
    }
    
    connectedCallback() {
        this.append(
            this._lobby
        )

        this.connect()
    }
}

customElements.define('chatclient-app', App);