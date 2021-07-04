import { HubConnection } from '@microsoft/signalr'
import { UserName } from './model.js'
import { GetParticipants } from './api.js'

export class UserSelect extends HTMLElement {
    _connection : HubConnection = undefined
    set Connection(c : HubConnection) {
        if (this._connection === undefined) {
            c.on('ClientConnected', this.addUserName.bind(this))
            c.on('ClientDisconnected', this.removeUserName.bind(this))
            this._connection = c
        }
    }
    
    _userNames : UserName[] = []

    get UserNames() {
        return this._userNames.slice()
    }

    set UserNames(ns : UserName[]) {
        this._userNames = ns
        this.renderUserNames()
    }

    initializeUserNames() {
        GetParticipants()
            .then(r => {
                let ns = this.UserNames
                this.UserNames = ns.concat(r)
            })
            .catch(err => console.error(err))
    }

    addUserName(n : UserName) {
        let ns = this.UserNames
        ns.unshift(n)
        this.UserNames = ns
    }

    removeUserName(n : UserName) {
        this.UserNames = this.UserNames.filter(x => x !== n )
    }

    _listid : string = 'chatclient-userSelect.list'
    _input : HTMLInputElement = undefined
    _dataList : HTMLDataListElement = undefined

    toOption(u : UserName) : HTMLOptionElement {
        let elt = document.createElement('option')
        elt.value = u
        elt.innerText = u

        return elt
    }

    renderUserNames() {
        this._dataList.innerText = ''
        this.UserNames.forEach(x => {
            this._dataList.append(
                this.toOption(x)
            )
        })
    }

    _counterPart : string = undefined
    set CounterPart(v : string) {
        this._counterPart = v
        this.renderOpenChannelBtn()
    }
    
    get CounterPart() {
        return this._counterPart
    }
    _openChannelBtn : HTMLButtonElement = undefined

    renderOpenChannelBtn() {
        this._openChannelBtn.disabled = this._counterPart == undefined
    }

    setCounterPart() {
        var input = this._input.value
        if (this.UserNames.some(x => x === input)) {
            this.CounterPart = this._input.value
        } else {
            this.CounterPart = undefined
        }
        
    }

    openChannel() {
        if (this.CounterPart == undefined) {
            return;
        }
        
        this._connection
            .send('OpenChannel', this.CounterPart, 'Hello')
            .then(x => console.log('Opened channel: ' + this.CounterPart))
            .catch(e => console.error(e))
    }

    constructor() {
        super()
        let dataList = document.createElement('datalist')
        dataList.id = this._listid
        
        let input = document.createElement('input')
        input.setAttribute('list', this._listid)
        input.type = 'text'
        input.name = 'counterPart'
        input.addEventListener('change', this.setCounterPart.bind(this))

        let openChannelBtn = document.createElement('button');
        openChannelBtn.innerText = "Say Hello"
        openChannelBtn.disabled = true
        openChannelBtn.addEventListener('click', this.openChannel.bind(this))
        
        this._dataList = dataList
        this._input = input
        this._openChannelBtn = openChannelBtn;
    }
    
    connectedCallback() {
        this.initializeUserNames()
        let label = document.createElement('label')
        label.innerText = 'Online'
        this.append(
            label,
            this._input,
            this._dataList,
            this._openChannelBtn
        )
    }
}

customElements.define('chatclient-userselect', UserSelect);