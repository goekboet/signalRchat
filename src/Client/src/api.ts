import { Message, UserName, ChannelId } from './model.js'

export async function GetBroadCast() : Promise<Message[]> {
    try {
        var r = await fetch('https://localhost:5001/api/broadcast')
        if(!r.ok) {
            throw new Error(r.statusText)
        }
        return await r.json();
    } catch (e) {
        console.error(e)
    }
}

export async function GetChannel(cId : ChannelId) : Promise<Message[]> {
    try {
        var r = await fetch(`api/channels/${cId}`)
        if(!r.ok) {
            throw new Error(r.statusText)
        }
        return await r.json();
    } catch (e) {
        console.error(e)
    }
}

export async function GetParticipants() : Promise<UserName[]> {
    try {
        var r = await fetch("https://localhost:5001/api/participants")
        if(!r.ok) {
            throw new Error(r.statusText)
        }
        return await r.json();
    } catch (e) {
        console.error(e)
    }
}

export async function GetOpenChannels() : Promise<string[]> {
    try {
        var r = await fetch('https://localhost:5001/api/openChannels')
        if (!r.ok) {
            throw new Error(r.statusText)
        }

        return await r.json()
    }
    catch (e) {
        console.error(e)
    }
}