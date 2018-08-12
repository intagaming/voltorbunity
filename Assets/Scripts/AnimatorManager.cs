using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour {
    [HideInInspector]
    public int numOfAnimations = 0;
    [HideInInspector]
    public List<string> names = new List<string>();
    [HideInInspector]
    public List<bool> foldouts = new List<bool>();
    [HideInInspector]
    public List<Animator> animators = new List<Animator>();

    public Animator getAnimator(string animatorName) {
        return animators[names.IndexOf(animatorName)];
    }

}
