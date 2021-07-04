import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { Lobby } from './lobby.js'
import { UserSelect } from './userSelect.js'

export class App extends HTMLElement {
    _connection : HubConnection
    _lobby : Lobby
    _userSelect : UserSelect

    connect() {
        this._connection.start().then(() => {
        this._lobby.Connection = this._connection
        this._userSelect.Connection = this._connection
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
        let userSelect = new UserSelect()

        this._lobby = lobby
        this._connection = connection;
        this._userSelect = userSelect
    }
    
    connectedCallback() {
        this.append(
            this._userSelect,
            this._lobby
        )

        this.connect()
    }
}

customElements.define('chatclient-app', App);