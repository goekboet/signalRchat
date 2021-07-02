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

export class Lobby extends HTMLElement {
    _container : HTMLDivElement
    _form : HTMLFormElement
    _messageLabel : HTMLLabelElement
    _messageInput : HTMLInputElement
    _messageSubmit : HTMLInputElement
    _messageList : HTMLDivElement

    _connection : HubConnection = undefined
    _messages : Message[] = []

    set Connection(c : HubConnection) {
        if (!this._connection) {
            this._messageSubmit.disabled = false;

            c.onreconnecting(() => {
                this._messageSubmit.disabled = true
            })

            c.onreconnected(() => {
                this._messageSubmit.disabled = false
            })

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
        this._messageList.innerText = ''
        this._messages
            .map(x => toMessageEntry(x))
            .forEach(x => this._messageList.append(x))

    }

    send() {
        this._connection
            .send('SendMessage', this._messageInput.value )
            .then(() => this._messageInput.value = '')
    }

    constructor() {
        super()
        let container = document.createElement('div')
        container.id = 'chatclient-container'

        let form = document.createElement('form')
        form.id = 'chatclient-send'

        let messageLabel = document.createElement('label')
        messageLabel.setAttribute('for', 'chatclient-message')
        messageLabel.innerText = 'Message'

        let messageInput = document.createElement('input')
        messageInput.name = 'chatclient-message'
        messageInput.type = 'text'

        let messageSubmit = document.createElement('input')
        messageSubmit.type = 'submit'
        messageSubmit.disabled = true;
        messageSubmit.addEventListener('click', evt => {
            evt.preventDefault()
            this.send()
        })

        let messageList = document.createElement('div')
        messageList.id = 'chatclient-messages'

        this._container = container
        this._form = form
        this._messageLabel = messageLabel
        this._messageSubmit = messageSubmit
        this._messageInput = messageInput
        this._messageList = messageList

    }

    connectedCallback() {
        this._form.append(
            this._messageLabel,
            this._messageInput,
            this._messageSubmit
        )

        this.append(
            this._form,
            this._messageList
        )

        this.addHistory()
    }
}

customElements.define('chatclient-lobby', Lobby);