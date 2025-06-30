ImGui.Unity
=====

[Dear ImGui](https://github.com/ocornut/imgui) wrapper for Unity (Yet another...), binding Dear ImGui [`v1.92.0-docking`](https://github.com/ocornut/imgui/releases/tag/v1.92.0)

## From Dear ImGui README:  
<sub>(This library is available under a free and permissive license, but needs financial support to sustain its continued improvements. In addition to maintenance and stability there are many desirable features yet to be added. If your company is using Dear ImGui, please consider reaching out.)</sub>

Businesses: support continued development and maintenance via invoiced sponsoring/support contracts:
<br>&nbsp;&nbsp;_E-mail: contact @ dearimgui dot com_
<br>Individuals: support continued development and maintenance [here](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WGHNC6MBFLZ2S). Also see [Funding](https://github.com/ocornut/imgui/wiki/Funding) page.

## Prior arts
This repository is a fork of following repositories:
* [dear-imgui-unity](https://github.com/realgamessoftware/dear-imgui-unity)
  * Base ImGui.NET Interop, Unity side mesh generation, Unity side rendering, input binding, etc...
* [uimgui](https://github.com/psydack/uimgui)
  * Mesh generation update, URP support, image supoprt, etc...
* [EchoImGui](https://github.com/Lithius0/EchoImGui)
  * URP Render Graph support

## Changes form prior arts
* Update to Dear ImGui 1.92.0 Docking
* Move update loop from MonoBehaviour to [PlayerLoopSystem](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/LowLevel.PlayerLoopSystem.html)
* Remove all Unity config ScriptableObjects, please config Dear ImGui via code

## Notice
* This is not a battle tested fork
* It only supports URP with Render Graph
* Currently there is no plan to expand RP support nor adding more functionality

## Installation
* Install git package with URL: `https://github.com/FrankNine/ImGui.Unity.git?path=/Packages/com.franknine.imgui.unity`
* Add Dear ImGui Renderer feature to URP Universal Renderer Data
* Register to `DearImGuiRendererFeature.OnLayout` event