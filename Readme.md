# Display and Record your Eye in Game

This mod aims record and display the eye tracking information in the beatleader replay file.

# Road map

Maybe it will not work. Just an idea fram the unreleased steam frame. Let's code first for fun.

- [ ] Get Eye tracking data from Unity runtime.
- [ ] Record the data to Beatleader's record file.
- [ ] Buy a headset that supports eye tracking???
- [ ] Release the first version of this mod.
- [ ] Display the eye area in `Camera 2` mod.
- [ ] Replay support.
- [ ] Release the seconed version of this mod.


- [ ] Impl these record (only) things in Meta Quest.
- [ ] Ask someone using Meta Quest Pro to make sure it works.
- [ ] Release the quest mod.

# Record Format

This mod uses Custom Mod ID `EyeTrackP`. I will not include the full mod name here, because I want other mods can reuse this ID in the future for replay compat.

```
version                 - int, always 1
count                   - int
{                       - repeat count times
    time                - float, timestamp
    confL               - float, confidence of left eye
    {x, y, z}           - 3 float, eye position left
    {x,y,z,w}           - 4 float, eye rotation left
    confR               - float, confidence of right eye
    {x, y, z}           - 3 float, eye position right
    {x,y,z,w}           - 4 float, eye rotation right
}

- most readers don't need the following data

etAgent                 - string(length + bytes), the mod ID and version number information

eyeCloseCount           - int
{                       - repeat eyeCloseCount times
    time                - float, timestamp
    argCount            - byte
    {                   - repeat argCount times
        leftClose       - float
        rightClose      - float
    }
}
```

```
eventId:
0 leftEyeClose          no args defined
1 rightEyeClose         no args defined
2 leftEyeOpen           no args defined
3 rightEyeOpen          no args defined
```

Always skip `eventArgSize` bytes, even if it is something like `leftEyeClose` which have no args defined.

if you see unknown event Id, just ignore them and skip the `args`.

In the future the event maybe reloaded with different arguments.
