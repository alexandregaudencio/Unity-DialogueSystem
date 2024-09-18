using System.Collections.Generic;
using DialogueSystem.Enumerations;
using UnityEngine;

// [CreateAssetMenu(fileName = "CharacterDialogueAnimations", menuName = "ScriptableObjects/CharactersDialogueAnimations", order = 1)]
public class CharacterDialogueAnimations : ScriptableObject
{
    [System.Serializable]
    public class CharacterAnimations
    {
        public DSActor characterName;
        public List<AnimationClip> dialogueAnimations = new List<AnimationClip>();
    }

    public List<CharacterAnimations> charactersAnimationsList = new List<CharacterAnimations>();

    public List<AnimationClip> GetAnimationsForCharacter(string characterName)
    {
        CharacterAnimations characterAnimations = charactersAnimationsList.Find(c => c.characterName.ToString() == characterName);
        return characterAnimations != null ? characterAnimations.dialogueAnimations : new List<AnimationClip>();
    }

    public AnimationClip GetAnimationClip(string actor, string animationName)
    {
        CharacterAnimations characterAnimations = charactersAnimationsList.Find(c => c.characterName.ToString() == actor);
        if (characterAnimations != null)
        {
            return characterAnimations.dialogueAnimations.Find(animation => animation.name == animationName);
        }
        return null;
    }


}
