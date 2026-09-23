using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//场景切换目标点
public class TransitionDestination : MonoBehaviour
{
    public enum DestinationTag {
        ENTER, A, B, C
    }

    public DestinationTag destinationTag;

}
