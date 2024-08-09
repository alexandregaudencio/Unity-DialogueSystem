using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDialogueAnimations", menuName = "ScriptableObjects/CharactersDialogueAnimations", order = 1)]
public class CharacterDialogueAnimations : ScriptableObject
{
    [System.Serializable]
    public class CharacterAnimations
    {
        public string characterName;
        public List<AnimationClip> dialogueAnimations = new List<AnimationClip>();
    }

    public List<CharacterAnimations> charactersAnimationsList = new List<CharacterAnimations>();

    public List<AnimationClip> GetAnimationsForCharacter(string characterName)
    {
        CharacterAnimations characterAnimations = charactersAnimationsList.Find(c => c.characterName == characterName);
        return characterAnimations != null ? characterAnimations.dialogueAnimations : new List<AnimationClip>();
    }
}
