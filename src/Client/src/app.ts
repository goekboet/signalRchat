import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { UserSelect } from './userSelect.js'
import { MessageInput } from './messageInput.js'
import { MessageList } from './messageList.js'
import { ChannelSelect } from './channelSelect.js'
import { ChannelId } from './model.js'
import { GetOpenChannels } from './api.js'

export class App extends HTMLElement {
    _connection : HubConnection = undefined
    _userSelect : UserSelect = undefined
    _openChannels : ChannelSelect[] = []
    get OpenChannels() {
        return this._openChannels
    }
    set OpenChannels(v : ChannelSelect[]) {
        this._openChannels = v
        this.renderChannels()
    }
    _messageInput : MessageInput = undefined
    _lobby : MessageList = undefined

    connect() {
        this._connection.start().then(() => {
            this._userSelect.Connection = this._connection
            this._messageInput.Connection = this._connection
            this._lobby.Connection = this._connection
            this.initialiazeChannels()
        }).catch(err => {
            console.error(err)
            this.connect()
        })
    }

    switchChannel(target : CustomEvent<ChannelId>) {
        for (const s of this.OpenChannels) {
            s.Selected = s.Channel === target.detail
        }
    }

    initialiazeChannels() {
        GetOpenChannels()
            .then((r : string[]) => {
                let lobby = new ChannelSelect()
                lobby.addEventListener('selectChannel', this.switchChannel.bind(this))
                lobby.Selected = true
                let selects = [ lobby ]
                r.forEach(x => {
                    let channel = new ChannelSelect()
                    channel.addEventListener('selectChannel', this.switchChannel.bind(this))
                    channel.Channel = x
                    channel.Connection = this._connection
                    selects.push(channel)
                })

                this.OpenChannels = selects
            })
            .catch(err => {
                console.error(err)
            })
    }

    channelOpened(cId : ChannelId) {
        let select = new ChannelSelect()
        select.Channel = cId
        select.Connection = this._connection

        let cs = this.OpenChannels
        cs.push(select)
        this.OpenChannels = cs    
    }

    channelClosed(cId : ChannelId) {
        this.OpenChannels = this.OpenChannels.filter(x => x.Channel !== cId)
    }

    _channelContainer : HTMLDivElement = undefined
    renderChannels() {
        this._channelContainer.innerText = ''
        for (const openChannel of this.OpenChannels) {
            this._channelContainer.appendChild(openChannel)
        }
    }

    constructor() {
        super()

        let connection = new HubConnectionBuilder()
            .withUrl('/chathub')
            .configureLogging(LogLevel.Information)
            .build()

        let userSelect = new UserSelect()
        let channelContainer = document.createElement('div') 
        channelContainer.id = 'chatclient-app-channelcontainer'
        connection.on('ChannelOpened', this.channelOpened.bind(this))
        connection.on('ChannelClosed', this.channelClosed.bind(this))

        let messageInput = new MessageInput()
        let lobby = new MessageList()

        this._connection = connection;
        this._userSelect = userSelect
        this._channelContainer = channelContainer
        this._messageInput = messageInput
        this._lobby = lobby
    }
    
    connectedCallback() {
        this.append(
            this._userSelect,
            this._channelContainer,
            this._messageInput,
            this._lobby
        )

        this.connect()
    }
}

customElements.define('chatclient-app', App);