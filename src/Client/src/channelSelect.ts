import { HubConnection } from '@microsoft/signalr'
import { ChannelId } from './model.js'

export class ChannelSelect extends HTMLElement {
    _selected : boolean = false
    get Selected() {
        return this._selected
    }

    set Selected(v : boolean) {
        this._selected = v
        this.classList.toggle('selected', this.Selected)
    }

    _channelId : ChannelId = ''
    set Channel(v : ChannelId) {
        this._channelId = v
    }

    get Channel() {
        return this._channelId
    }

    _lastTouched : number = 0
    get LastTouched() {
        return this._lastTouched
    }
    set LastTouched(v : number) {
        this._lastTouched = v
    }

    _unread : number = 0
    get Unread() {
        return this._unread
    }
    set Unead(v : number) {
        this._unread = v
    }


    emitSelectEvent() {
        let evt = new CustomEvent<ChannelId>('selectChannel', { detail: this.Channel }) 
        this.dispatchEvent(evt)
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
    get Connection() {
        return this._connection
    }

    closeChannel() {
        console.log('Closing ' + this.Channel)
        this.Connection.send('CloseChannel', this.Channel).then(() => { console.log('Closed')})
    }

    constructor() {
        super()
        let selectButton = document.createElement('button')
        selectButton.name = 'select'
        selectButton.addEventListener('click', this.emitSelectEvent.bind(this))
        
        let closeButton = document.createElement('button')
        closeButton.name = 'close'
        closeButton.addEventListener('click', this.closeChannel.bind(this))

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