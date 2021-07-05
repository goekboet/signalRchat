import { HubConnection } from '@microsoft/signalr'
import { Message } from './model.js'
import { GetBroadCast } from './api'

function toMessageEntry(m : Message) {
    let container = document.createElement('div')
    container.classList.add('message')

    let sender = document.createElement('h1')
    sender.innerText = `${m.unixMsTimestamp}: ${m.sender}`

    let body = document.createElement('p')
    body.innerText = m.payload

    container.append(sender, body)

    return container

}

export class MessageList extends HTMLElement {
    _connection : HubConnection = undefined
    
    _messages : Message[] = []

    set Connection(c : HubConnection) {
        if (!this._connection) {
            c.on('ReceiveMessage', (m : Message) => {
                this._messages.unshift(m)
                this._messages = this._messages.slice(0, 25)
                this.renderMessages()
            })
            this._connection = c
        }
    }

    addHistory() {
        GetBroadCast().then(x => {
            this._messages = this._messages.concat(x)
            this.renderMessages()
        })
    }

    renderMessages() {
        this.innerText = ''
        this._messages
            .map(x => toMessageEntry(x))
            .forEach(x => this.append(x))

    }

    constructor() {
        super()
    }

    connectedCallback() {
        this.addHistory()
    }
}

customElements.define('chatclient-messagelist', MessageList);