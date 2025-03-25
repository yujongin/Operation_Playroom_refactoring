using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct LobbyRoomPlayerData : INetworkSerializable, IEquatable<LobbyRoomPlayerData>
{
    public FixedString512Bytes authId; 
    public ulong clientId;
    public FixedString128Bytes userName;
    public bool isReady;
    public bool isLeader;
    public int team;
    public int role;

    public bool Equals(LobbyRoomPlayerData other)
    {
        return authId == other.authId
            && clientId == other.clientId 
            && userName == other.userName 
            && isReady == other.isReady 
            && isLeader == other.isLeader
            && team == other.team
            && role == other.role;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref authId);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref userName);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref isLeader);
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref role);
    }
}
