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
    
    _messages : Message[] = []
    set Messages(v : Message[]) {
        this._messages = v
        this.renderMessages()
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
        this.renderMessages()
    }
}

customElements.define('chatclient-messagelist', MessageList);