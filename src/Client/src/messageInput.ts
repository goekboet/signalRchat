import { HubConnection } from '@microsoft/signalr'
import { ChannelId } from './model.js'

export class MessageInput extends HTMLElement {
    _channel : ChannelId = ""
    get Channel() {
        return this._channel
    }
    _messageInput : HTMLInputElement
    _messageSubmit : HTMLButtonElement

    _connection : HubConnection = undefined

    set Connection(c : HubConnection) {
        if (!this._connection) {
            this._messageSubmit.disabled = false;

            c.onreconnecting(() => {
                this._messageSubmit.disabled = true
            })

            c.onreconnected(() => {
                this._messageSubmit.disabled = false
            })

            this._connection = c
        }
    }

    send() {
        this._connection
            .send('SendMessage', this._messageInput.value, this.Channel )
            .then(() => this._messageInput.value = '')
    }

    constructor() {
        super()

        let messageInput = document.createElement('input')
        messageInput.name = 'message'
        messageInput.type = 'text'

        let messageSubmit = document.createElement('button')
        messageSubmit.innerText = 'Send'
        messageSubmit.disabled = true;
        messageSubmit.addEventListener('click', this.send.bind(this))

        this._messageInput = messageInput
        this._messageSubmit = messageSubmit
    }
    
    connectedCallback() {
        this.append(
            this._messageInput,
            this._messageSubmit
        )
    }
}

customElements.define('chatclient-messageinput', MessageInput);