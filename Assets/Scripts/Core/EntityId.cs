using System;

namespace STS.Core
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        private static int _nextId = 1;

        public int Value { get; }

        public EntityId(int value) => Value = value;

        public static EntityId New() => new EntityId(_nextId++);

        public static void ResetIdCounter() => _nextId = 1;

        public bool Equals(EntityId other) => Value == other.Value;

        public override bool Equals(object obj) => obj is EntityId other && Equals(other);

        public override int GetHashCode() => Value;

        public override string ToString() => $"Entity#{Value}";
    }
}
