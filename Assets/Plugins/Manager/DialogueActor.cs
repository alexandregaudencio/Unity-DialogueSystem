using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueActor : MonoBehaviour
{
    [SerializeField] DialogueUIManager dialogueUIManager;
    public CharacterDialogueAnimations characterDialogueAnimations; // modificar para receber o speech animation diretamente do graphview como animation clip 
    public Animator animator;

    public string ActorName;
    public bool Active;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (dialogueUIManager == null) Debug.Log("dialogueisNull");
        dialogueUIManager.OnDialogueChanged += EvaluateAnimation;
    }

    void EvaluateAnimation(string Actor, string SpeechAnimation)
    {
        if (Active)
        {
            if (ActorName == Actor)
            {
                AnimationClip animationTest = characterDialogueAnimations.GetAnimationClip(ActorName, SpeechAnimation);
                ChangeAnimation(animationTest);
            }
            else
            {
                AnimationClip animationTest = characterDialogueAnimations.GetAnimationClip(ActorName, "listening");
                ChangeAnimation(animationTest);
            }
        }
    }


    public void InitializeActor(string Actor, string Animation)
    {
        Active = true;
        ActorName = Actor;
        // Debug.Log($"{Animation} Animation initializar");
        // if (characterDialogueAnimations.GetAnimationClip(ActorName, Animation)) Debug.Log("Oiii apareci"); //* Funcionando
        AnimationClip animationTest = characterDialogueAnimations.GetAnimationClip(ActorName, Animation);
        ChangeAnimation(animationTest);
    }

    void ChangeAnimation(AnimationClip newClip)
    {
        if (newClip == null)
        {
            Debug.LogError("New animation clip is null!");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator runtime controller is null!");
            return;
        }

        var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        if (overrideController.runtimeAnimatorController.animationClips.Length == 0)
        {
            Debug.LogError("No animation clips found in the animator controller!");
            return;
        }

        var currentClip = overrideController.runtimeAnimatorController.animationClips[0];
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        new KeyValuePair<AnimationClip, AnimationClip>(currentClip, newClip)
    };

        overrideController.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = overrideController;
        animator.Play("default", 0, 0);
    }

}
