import { Message } from './model.js'

function toMessageEntry(m : Message) {
    let container = document.createElement('div')
    container.classList.add('message')

    let sender = document.createElement('h1')
    let ts = new Date(m.unixMsTimestamp).toLocaleTimeString()
    sender.innerText = `${ts} - ${m.sender}`

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