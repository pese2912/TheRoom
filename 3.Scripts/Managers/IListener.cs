using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVENT_TYPE
{
    SURVIVOR_CREATE,SURVIVOR_HIT, SURVIVOR_MOVE, SURVIVOR_STOP, SURVIVOR_GRAMCTRL, GRAM_START, GRAM_STOP
};

public interface IListener
{

    void OnEvent(EVENT_TYPE Evenet_Type, Component Sender, object Parm = null);

}