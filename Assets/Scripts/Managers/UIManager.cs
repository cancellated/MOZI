using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 用户界面管理系统，负责：
/// 1. 全局UI元素的生命周期管理
/// 2. 字体和基础UI样式的统一配置
/// 3. 界面切换的动画控制
/// </summary>
public class UIManager : SingletonBase<UIManager>
{
    #region 事件中枢
    protected override void Initialize()
    {
        
    }
    #endregion
}