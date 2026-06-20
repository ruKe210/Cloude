using UnityEngine;

namespace STS.Presentation
{
    [CreateAssetMenu(fileName = "OfficeBattleArtSet", menuName = "STS/Office Battle Art Set")]
    public class OfficeBattleArtSet : ScriptableObject
    {
        public Sprite background;
        public Sprite playerPortrait;
        public Sprite enemyPortrait;
        public Sprite[] playerIdleFrames;
        public Sprite playerAttackFrame;
        public Sprite[] enemyIdleFrames;
        public Sprite enemyAttackFrame;
        public RuntimeAnimatorController playerAnimator;
        public RuntimeAnimatorController enemyAnimator;
    }
}
