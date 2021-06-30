import { Message } from './model.js'

export async function GetBroadCast() : Promise<Message[]> {
    try {
        var r = await fetch("https://localhost:5001/api/broadcast")
        if(!r.ok) {
            throw new Error(r.statusText)
        }
        return await r.json();
    } catch (e) {
        console.error(e)
    }
}