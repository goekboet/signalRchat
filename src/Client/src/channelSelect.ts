import { HubConnection } from '@microsoft/signalr'
import { ChannelId } from './model.js'

export class ChannelSelect extends HTMLElement {
    _channelId : ChannelId = ''
    set Channel(v : ChannelId) {
        this._channelId = v
    }
    get Channel() {
        return this._channelId
    }

    _selectButton : HTMLButtonElement = undefined
    _closeButton : HTMLButtonElement = undefined
    render() {
        this._selectButton.innerText = this.Channel === '' 
            ? 'lobby' 
            : this.Channel
        this._closeButton.innerText = '×'
        this._closeButton.disabled = this.Channel === ''
    }

    _connection : HubConnection = undefined
    set Connection(c : HubConnection) {
        if (!this._connection) {
            this._connection = c
        }
    }

    constructor() {
        super()
        let selectButton = document.createElement('button')
        selectButton.name = 'select'
        let closeButton = document.createElement('button')
        closeButton.name = 'close'

        this._selectButton = selectButton
        this._closeButton = closeButton
    }
    
    connectedCallback() {
        this.append(
            this._selectButton,
            this._closeButton
        )
        this.render()
    }
}

customElements.define('chatclient-channelselect', ChannelSelect);