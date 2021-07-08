import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { UserSelect } from './userSelect.js'
import { MessageInput } from './messageInput.js'
import { MessageList } from './messageList.js'
import { ChannelSelect } from './channelSelect.js'
import { ChannelId, Message } from './model.js'
import { GetBroadCast, GetChannel, GetOpenChannels } from './api.js'

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

    getSelectedChannel() : ChannelId {
        return this.OpenChannels.find(x => x.Selected)?.Channel
    }

    _messageInput : MessageInput = undefined
    _selectedChannel : ChannelId = ''
    get SelectedChannel() {
        return this._selectedChannel
    }
    set SelectedChannel(v : ChannelId) {
        this._selectedChannel = v
    }

    _displayedMessages : MessageList = undefined

    _messageCache : Message[] = []
    get MessageCache() {
        return this._messageCache.slice()
    }
    set MessageCache(v : Message[]) {
        this._messageCache = v
        if (this._displayedMessages) {
            this._displayedMessages.Messages = v
        }
    }

    connect() {
        this._connection.start().then(() => {
            this._userSelect.Connection = this._connection
            this._messageInput.Connection = this._connection
            
            
        }).catch(err => {
            console.error(err)
            this.connect()
        })
    }

    switchChannel(target : CustomEvent<ChannelId>) {
        this.SelectedChannel = target.detail
        this._messageInput.Channel = target.detail
        for (const c of this.OpenChannels) {
            if (c.Channel === this.SelectedChannel) {
                c.Selected = true
                c.Unead = 0
            } else {
                c.Selected = false
            }
        }
        this.initializeMessages()
    }

    handleMessage(m : Message) {
        if (m.channel === this.SelectedChannel) {
            let msgs = this.MessageCache
            msgs.unshift(m)
            this.MessageCache = msgs
        } else {
            for (const c of this.OpenChannels) {
                if (c.Channel === m.channel) {
                    c.LastTouched = m.unixMsTimestamp
                    c.Unead += 1
                }
            }
        }
    } 

    initializeMessages() {
        if (this.SelectedChannel === '') {
            GetBroadCast().then(history => {
                this.MessageCache = history
            })
        } else {
            GetChannel(this.SelectedChannel).then(channel => {
                this.MessageCache = channel
            })
        }
    }

    createChannelSelect(channel : ChannelId) {
        var select = new ChannelSelect()
        select.Channel = channel
        select.Selected = channel === this.SelectedChannel
        select.addEventListener('selectChannel', this.switchChannel.bind(this))
        select.Connection = this._connection

        return select
    }

    channelOpened(cId : ChannelId) {
        let select = this.createChannelSelect(cId)

        let cs = this.OpenChannels
        cs.push(select)
        this.OpenChannels = cs    
    }

    channelClosed(cId : ChannelId) {
        this.OpenChannels = this.OpenChannels.filter(x => x.Channel !== cId)
    }

    initializeChannels() {
        var lobby = this.createChannelSelect('')

        this.OpenChannels = [ lobby ]
        GetOpenChannels().then(cs => {
            for (const c of cs) {
                let select = this.createChannelSelect(c)
                
                let openChannels = this.OpenChannels
                openChannels.push(select)
                this.OpenChannels = openChannels
            }
        })
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
        this._displayedMessages = lobby
        this._connection.on('NewChatmessage', this.handleMessage.bind(this))
    }
    
    connectedCallback() {
        this.append(
            this._userSelect,
            this._channelContainer,
            this._messageInput,
            this._displayedMessages
        )

        this.connect()
        this.initializeChannels()
        this.initializeMessages()
    }
}

customElements.define('chatclient-app', App);