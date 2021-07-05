import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { UserSelect } from './userSelect.js'
import { MessageInput } from './messageInput.js'
import { MessageList } from './messageList.js'

export class App extends HTMLElement {
    _connection : HubConnection = undefined
    _userSelect : UserSelect = undefined
    _messageInput : MessageInput = undefined
    _lobby : MessageList = undefined

    connect() {
        this._connection.start().then(() => {
            this._userSelect.Connection = this._connection
            this._messageInput.Connection = this._connection
            this._lobby.Connection = this._connection
        }).catch(err => {
            console.error(err)
            this.connect()
        })
        
    }

    constructor() {
        super()

        let connection = new HubConnectionBuilder()
            .withUrl('/chathub')
            .configureLogging(LogLevel.Information)
            .build()

        let userSelect = new UserSelect()
        let messageInput = new MessageInput()
        let lobby = new MessageList()

        this._connection = connection;
        this._userSelect = userSelect
        this._messageInput = messageInput
        this._lobby = lobby
    }
    
    connectedCallback() {
        this.append(
            this._userSelect,
            this._messageInput,
            this._lobby
        )

        this.connect()
    }
}

customElements.define('chatclient-app', App);