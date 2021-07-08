export interface Message {
    readonly unixMsTimestamp : number
    readonly channel : string
    readonly sender : UserName
    readonly payload : string
}

export type UserName = string
export type ChannelId = string

