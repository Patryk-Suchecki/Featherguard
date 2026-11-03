BIRD = dict(root="root", body="body", head="head", main="wing.R", off="wing.L",
            weapon="weapon", offweapon="offhand", tail="tail", tail2=None, lift=0.36)

LIZARD = dict(root="root", body="torso", head="head", main="arm.R", off="arm.L",
              weapon="weapon", offweapon="offhand", tail="tail", tail2="tail2", lift=0.26)


def _pose(p, **bones):
    pose = {}
    for role, values in bones.items():
        bone = p.get(role)
        if bone:
            pose[bone] = values
    return pose


def idle(p):
    breathe = _pose(p,
                    body=dict(scl=(1.04, 0.95, 1.04)),
                    head=dict(rot=(4, 0, 0)),
                    main=dict(rot=(0, 0, -6)),
                    off=dict(rot=(0, 0, 6)),
                    tail=dict(rot=(0, 8, 0)),
                    tail2=dict(rot=(0, 12, 0)))
    return [(1, {}), (21, breathe), (41, {})], {}


def melee(p):
    windup = _pose(p,
                   body=dict(rot=(-14, 0, 0), scl=(0.96, 1.06, 0.96)),
                   head=dict(rot=(-8, 0, 0)),
                   main=dict(rot=(-160, 0, 0)),
                   weapon=dict(rot=(60, 0, 0)),
                   off=dict(rot=(20, 0, 0)))
    strike = _pose(p,
                   body=dict(rot=(18, 0, 0), loc=(0, 0, 0.06), scl=(1.08, 0.9, 1.08)),
                   head=dict(rot=(10, 0, 0)),
                   main=dict(rot=(-35, 0, 0)),
                   weapon=dict(rot=(130, 0, 0)),
                   off=dict(rot=(-20, 0, 0)))
    follow = _pose(p,
                   body=dict(rot=(22, 0, 0), loc=(0, 0, 0.08), scl=(1.04, 0.95, 1.04)),
                   head=dict(rot=(8, 0, 0)),
                   main=dict(rot=(-10, 0, 0)),
                   weapon=dict(rot=(110, 0, 0)))
    settle = _pose(p,
                   body=dict(rot=(4, 0, 0)),
                   main=dict(rot=(-5, 0, 0)),
                   weapon=dict(rot=(20, 0, 0)))
    return [(1, {}), (5, windup), (8, strike), (12, follow), (20, settle), (26, {})], {"OnHitFrame": 8}


def shoot(p):
    aim = _pose(p,
                body=dict(rot=(-4, 0, 0)),
                off=dict(rot=(-85, 0, 0)),
                offweapon=dict(rot=(85, 0, 0)),
                main=dict(rot=(-80, 0, 0)))
    draw = _pose(p,
                 body=dict(rot=(-8, 0, 0), scl=(0.97, 1.04, 0.97)),
                 off=dict(rot=(-88, 0, 0)),
                 offweapon=dict(rot=(88, 0, 0)),
                 main=dict(rot=(-75, 0, 0), loc=(0, 0, -0.12)))
    release = _pose(p,
                    body=dict(rot=(-12, 0, 0), scl=(1.05, 0.95, 1.05)),
                    off=dict(rot=(-88, 0, 0)),
                    offweapon=dict(rot=(88, 0, 0)),
                    main=dict(rot=(-40, 0, 0), loc=(0, 0, -0.18)))
    recoil = _pose(p,
                   body=dict(rot=(-6, 0, 0)),
                   off=dict(rot=(-70, 0, 0)),
                   offweapon=dict(rot=(70, 0, 0)),
                   main=dict(rot=(-30, 0, 0), loc=(0, 0, -0.1)))
    return [(1, {}), (7, aim), (13, draw), (15, release), (19, recoil), (30, {})], {"OnHitFrame": 15}


def cast(p):
    raise_ = _pose(p,
                   body=dict(rot=(-10, 0, 0), scl=(0.94, 1.08, 0.94)),
                   head=dict(rot=(-10, 0, 0)),
                   main=dict(rot=(-150, 0, 0)),
                   weapon=dict(rot=(140, 0, 0)),
                   off=dict(rot=(-60, 0, 0)))
    shake = _pose(p,
                  body=dict(rot=(-8, 0, 0), scl=(0.96, 1.06, 0.96)),
                  head=dict(rot=(-12, 0, 0)),
                  main=dict(rot=(-145, 0, 0)),
                  weapon=dict(rot=(135, 0, 0)),
                  off=dict(rot=(-65, 0, 0)))
    release = _pose(p,
                    body=dict(rot=(12, 0, 0), loc=(0, 0, 0.04), scl=(1.07, 0.92, 1.07)),
                    head=dict(rot=(6, 0, 0)),
                    main=dict(rot=(-80, 0, 0)),
                    weapon=dict(rot=(120, 0, 0)),
                    off=dict(rot=(-30, 0, 0)))
    hold = _pose(p,
                 body=dict(rot=(6, 0, 0)),
                 main=dict(rot=(-60, 0, 0)),
                 weapon=dict(rot=(80, 0, 0)))
    return [(1, {}), (9, raise_), (12, shake), (16, release), (22, hold), (30, {})], {"OnHitFrame": 16}


def hit(p):
    recoil = _pose(p,
                   body=dict(rot=(-22, 0, 0), loc=(0, 0, -0.06), scl=(1.1, 0.88, 1.1)),
                   head=dict(rot=(-18, 0, 0)),
                   main=dict(rot=(-40, 0, 0)),
                   off=dict(rot=(-40, 0, 0)))
    recover = _pose(p,
                    body=dict(rot=(-8, 0, 0), scl=(0.97, 1.04, 0.97)),
                    head=dict(rot=(-4, 0, 0)))
    return [(1, {}), (3, recoil), (7, recover), (14, {})], {}


def death(p):
    stagger = _pose(p,
                    body=dict(rot=(-25, 0, 0), scl=(1.08, 0.9, 1.08)),
                    head=dict(rot=(-20, 0, 0)),
                    main=dict(rot=(-50, 0, 0)),
                    off=dict(rot=(-50, 0, 0)))
    lift = p["lift"]
    fallen = _pose(p,
                   root=dict(rot=(-88, 0, 0), loc=(0, lift, 0)),
                   body=dict(scl=(1.1, 0.85, 1.1)),
                   head=dict(rot=(-10, 0, 0)),
                   main=dict(rot=(-120, 0, 0)),
                   off=dict(rot=(-120, 0, 0)))
    rest = _pose(p,
                 root=dict(rot=(-88, 0, 0), loc=(0, lift, 0)),
                 head=dict(rot=(-14, 0, 0)),
                 main=dict(rot=(-110, 0, 0)),
                 off=dict(rot=(-110, 0, 0)))
    return [(1, {}), (5, stagger), (14, fallen), (18, rest), (30, rest)], {}


def victory(p):
    crouch = _pose(p,
                   body=dict(scl=(1.1, 0.86, 1.1)),
                   main=dict(rot=(-20, 0, 0)),
                   off=dict(rot=(-20, 0, 0)))
    jump = _pose(p,
                 root=dict(loc=(0, 0.35, 0)),
                 body=dict(scl=(0.92, 1.1, 0.92)),
                 head=dict(rot=(-12, 0, 0)),
                 main=dict(rot=(-165, 0, 0)),
                 weapon=dict(rot=(165, 0, 0)),
                 off=dict(rot=(-165, 0, 0)),
                 offweapon=dict(rot=(165, 0, 0)))
    land = _pose(p,
                 root=dict(loc=(0, 0.02, 0)),
                 body=dict(scl=(1.08, 0.9, 1.08)),
                 main=dict(rot=(-150, 0, 0)),
                 weapon=dict(rot=(150, 0, 0)),
                 off=dict(rot=(-150, 0, 0)),
                 offweapon=dict(rot=(150, 0, 0)))
    return [(1, {}), (7, crouch), (13, jump), (19, land), (25, crouch), (31, {})], {}


def build_set(profile, attack):
    return {
        "Idle": idle(profile) + (True,),
        "Attack": attack(profile) + (False,),
        "Cast": cast(profile) + (False,),
        "Hit": hit(profile) + (False,),
        "Death": death(profile) + (False,),
        "Victory": victory(profile) + (True,),
    }
