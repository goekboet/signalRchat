export interface Message {
    readonly unixMsTimestamp : number
    readonly sender : string
    readonly payload : string
}

export type UserName = string
export type ChannelId = string

