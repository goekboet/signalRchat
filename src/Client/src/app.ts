import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

interface Message {
    readonly from : string
    readonly payload : string
}

function toMessageEntry(m : Message) {
    let container = document.createElement('div')
    container.classList.add('message')

    let sender = document.createElement('h1')
    sender.innerText = m.from

    let body = document.createElement('p')
    body.innerText = m.payload

    container.append(sender, body)

    return container

}

export class App extends HTMLElement {
    _container : HTMLDivElement
    _form : HTMLFormElement
    _messageLabel : HTMLLabelElement
    _messageInput : HTMLInputElement
    _messageSubmit : HTMLInputElement
    _messageList : HTMLDivElement

    _connection : HubConnection

    connect() {
        this._connection.start().then(() => {
            console.log('SignalR connected.')
            this._messageSubmit.disabled = false
        }).catch(err => {
            console.log(err)
            this._messageSubmit.disabled = true
            this.connect()
        })
    }

    send() {
        this._connection
            .send('SendMessage', { from: 'Testclient', payload: this._messageInput.value })
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

        let connection = new HubConnectionBuilder()
            .withUrl('/chathub')
            .configureLogging(LogLevel.Information)
            .build()

        connection.onclose(() => {
            messageSubmit.disabled = true
            this.connect()
        })

        connection.onreconnecting(() => {
            messageSubmit.disabled = true
        })

        connection.onreconnected(() => {
            messageSubmit.disabled = false
        })

        connection.on('ReceiveMessage', (m : Message) => {
            let entry = toMessageEntry(m)
            this._messageList.prepend(entry)
        })

        this._container = container
        this._form = form
        this._messageLabel = messageLabel
        this._messageSubmit = messageSubmit
        this._messageInput = messageInput
        this._messageList = messageList

        this._connection = connection;
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

        this.connect()
    }
}

customElements.define('chatclient-app', App);