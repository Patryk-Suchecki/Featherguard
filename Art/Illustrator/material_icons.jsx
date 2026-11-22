#target illustrator

// Draws every rarity frame and material icon as editable vectors in a new document and exports 256 px PNGs to Assets/Art/UI/Materials.
var OUT = new File($.fileName).parent.parent.parent.fsName.replace(/\\/g, "/") + "/Assets/Art/UI/Materials/";
#include "icon_kit.jsx"

var ROWS = [
    [["Frame_Common", "common", null], ["Frame_Uncommon", "uncommon", null], ["Frame_Rare", "rare", null], ["Frame_Epic", "epic", null], ["Frame_Legendary", "legendary", null]],
    [["Flax", "common", drawFlax], ["WolfPelt", "uncommon", drawWolfPelt], ["LastSheaf", "rare", drawLastSheaf]],
    [["MarshHerbs", "common", drawMarshHerbs], ["Amber", "uncommon", drawAmber], ["WispInAJar", "rare", drawWispInAJar]],
    [["Brimstone", "common", drawBrimstone], ["MountainOre", "uncommon", drawMountainOre], ["Thunderstone", "rare", drawThunderstone]],
    [["YewWood", "common", drawYewWood], ["TinderFungus", "uncommon", drawTinderFungus], ["OldOakHeartwood", "rare", drawOldOakHeartwood]],
    [["BoneAsh", "common", drawBoneAsh], ["GromnicaWax", "uncommon", drawGromnicaWax], ["NavEssence", "rare", drawNavEssence]],
    [["FirebirdFeather", "epic", drawFirebirdFeather], ["DeadWater", "epic", drawDeadWater], ["SerpentKingsCrown", "epic", drawSerpentKingsCrown], ["KupalaDew", "epic", drawKupalaDew]],
    [["FernFlower", "legendary", drawFernFlower], ["LivingWater", "legendary", drawLivingWater], ["AlatyrStone", "legendary", drawAlatyrStone]]
];

buildIcons(ROWS, OUT);

// ---------- Rye Fields ----------

function petals(c, x, y, r, color) {
    for (var a = 0; a < 5; a++) {
        var ang = a * 72 * Math.PI / 180;
        var petal = paint(oval(c, x + Math.sin(ang) * r * 0.7, y - Math.cos(ang) * r * 0.7, r * 0.5, r * 0.72), color, OUTLINE, 3.5);
        petal.rotate(-a * 72);
    }
    paint(circle(c, x, y, r * 0.32), "#F2D26A", OUTLINE, 3);
}

function drawFlax(c) {
    var tips = [[-38, -44], [-14, -62], [10, -66], [30, -50], [46, -26]];
    for (var i = 0; i < tips.length; i++)
        line(c, [[tips[i][0] * 0.2, 80], [tips[i][0] * 0.45, 16], tips[i]], i % 2 ? "#C9B77A" : "#A39A5B", 6, true);
    paint(roundRect(c, 0, 34, 44, 18, 6), "#6E6152", OUTLINE, 5);
    paint(roundRect(c, -6, 30, 18, 5, 2), "#9C8B70", null);
    petals(c, -38, -56, 18, "#7C9BE6");
    petals(c, 10, -78, 20, "#7C9BE6");
    petals(c, 46, -38, 16, "#7C9BE6");
}

function drawWolfPelt(c) {
    var pelt = [[0, -82], [16, -64], [34, -60], [74, -66], [52, -30], [44, 10], [70, 44], [34, 46], [14, 66], [4, 90], [-12, 66], [-34, 46], [-70, 44], [-44, 10], [-52, -30], [-74, -66], [-34, -60], [-16, -64]];
    paint(smooth(c, pelt), "#7E7A78", OUTLINE, 7);
    paint(smooth(c, [[0, -78], [-16, -62], [-34, -58], [-66, -62], [-48, -28], [-40, 10], [-62, 40], [-32, 42], [-12, 62], [0, 80], [-4, 20]]), "#ABA59D", null);
    paint(smooth(c, [[4, -60], [34, -56], [62, -60], [46, -28], [40, 10], [62, 40], [32, 42], [14, 60], [10, 10]]), "#55525E", null, 0, 75);
    paint(smooth(c, [[-10, -74], [0, -92], [10, -74]]), "#7E7A78", OUTLINE, 5);
    paint(smooth(c, [[0, -70], [6, -30], [4, 20], [0, 60], [-4, 20], [-6, -30]]), "#4E4A52", null, 0, 70);
    var marks = [[[-20, -30], [-8, -24]], [[10, -8], [22, -2]], [[-18, 18], [-6, 24]], [[8, 40], [18, 46]]];
    for (var i = 0; i < marks.length; i++) paint(shape(c, marks[i], false), null, "#3A3640", 4);
}

function drawLastSheaf(c) {
    glow(c, 0, -30, 70, "#F2DFB8");
    for (var i = -3; i <= 3; i++) {
        var tip = [i * 16, -40 - (3 - Math.abs(i)) * 8];
        line(c, [[i * 4, 84], [i * 6, 20], tip], i % 2 ? "#C9B77A" : "#A39A5B", 6, true);
        var ear = paint(oval(c, tip[0], tip[1] - 14, 8, 20), "#E2CF8C", OUTLINE, 3.5);
        ear.rotate(-i * 8);
        paint(oval(c, tip[0] - 2, tip[1] - 18, 3, 9), "#FFF1C2", null, 0, 80);
    }
    paint(roundRect(c, 0, 30, 58, 16, 6), "#B0402F", OUTLINE, 5);
    paint(shape(c, [[-6, 36], [-22, 64], [-8, 60], [0, 40], [8, 60], [22, 64], [6, 36]]), "#B0402F", OUTLINE, 4);
    paint(roundRect(c, -8, 26, 24, 4, 2), "#E0705C", null);
}

// ---------- Marshes ----------

function leaf(c, x, y, rx, ry, angle, color) {
    var l = paint(oval(c, x, y, rx, ry), color, OUTLINE, 3.5);
    l.rotate(angle);
    return l;
}

function drawMarshHerbs(c) {
    var stems = [[[-6, 84], [-22, 20], [-44, -50]], [[0, 84], [2, 10], [6, -70]], [[6, 84], [24, 20], [48, -44]]];
    for (var i = 0; i < stems.length; i++) line(c, stems[i], "#55613A", 6, true);
    var leaves = [[-30, 0, 40], [-14, 30, 30], [-40, -30, 50], [16, -20, -30], [-8, -40, 20], [30, 10, -40], [36, -20, -50], [10, 30, -20]];
    for (var k = 0; k < leaves.length; k++) leaf(c, leaves[k][0], leaves[k][1], 9, 22, leaves[k][2], k % 2 ? "#7D8460" : "#6A7748");
    var blooms = [[-44, -56], [6, -78], [48, -50]];
    for (var b = 0; b < blooms.length; b++)
        for (var d = 0; d < 5; d++) paint(circle(c, blooms[b][0] + Math.cos(d * 1.26) * 9, blooms[b][1] + Math.sin(d * 1.26) * 9, 6), "#E8E4D0", OUTLINE, 2.5);
    paint(roundRect(c, 0, 52, 40, 12, 5), "#7A6A4E", OUTLINE, 4);
}

function drawAmber(c) {
    var chunk = [[-58, -20], [-30, -72], [22, -78], [66, -34], [58, 36], [10, 74], [-48, 50]];
    paint(shape(c, chunk), "#D98A2B", OUTLINE, 8);
    paint(shape(c, [[10, 74], [58, 36], [66, -34], [20, -6]]), "#A9622C", null);
    paint(shape(c, [[-58, -20], [-30, -72], [22, -78], [6, -30], [-30, -10]]), "#F4BE5C", null);
    paint(shape(c, [[-38, -44], [-22, -62], [-14, -56], [-30, -38]]), "#FFE9B0", null, 0, 85);
    paint(oval(c, 6, 22, 11, 16), "#5A3215", null, 0, 70);
    paint(circle(c, 6, 1, 7), "#5A3215", null, 0, 70);
    var legs = [[-12, 10], [-14, 24], [-12, 38], [24, 10], [26, 24], [24, 38]];
    for (var i = 0; i < legs.length; i++) paint(shape(c, [[6, legs[i][1] - 4], legs[i]], false), null, "#5A3215", 3, 70);
}

function drawWispInAJar(c) {
    paint(roundRect(c, 0, 14, 104, 124, 34), "#8FA7A4", OUTLINE, 7, 55);
    glow(c, 0, 18, 44, "#8FE0C8");
    paint(smooth(c, [[0, -8], [18, 14], [12, 40], [-12, 40], [-18, 14]]), "#8FE0C8", null, 0, 85);
    paint(circle(c, 0, 24, 11), "#E6FFF6", null);
    sparkle(c, -22, -8, 10);
    sparkle(c, 24, 46, 8);
    paint(roundRect(c, -34, 10, 10, 80, 5), "#FFFFFF", null, 0, 35);
    paint(roundRect(c, 0, -52, 70, 18, 6), "#8FA7A4", OUTLINE, 5, 80);
    paint(roundRect(c, 0, -70, 52, 26, 8), "#8A6A48", OUTLINE, 5);
    paint(roundRect(c, -8, -76, 26, 6, 3), "#B08C64", null);
}

// ---------- Bald Mountain ----------

function crystal(c, x, y, w, h, angle, color, light, dark) {
    var body = [[x - w, y], [x - w, y - h], [x, y - h - w], [x + w, y - h], [x + w, y]];
    var whole = paint(shape(c, body), color, OUTLINE, 5);
    var lit = paint(shape(c, [[x - w, y], [x - w, y - h], [x, y - h - w], [x, y]]), light, null);
    var shade = paint(shape(c, [[x + w * 0.4, y], [x + w * 0.4, y - h], [x + w, y - h], [x + w, y]]), dark, null);
    var group = c.layer.groupItems.add();
    shade.move(group, ElementPlacement.PLACEATBEGINNING);
    lit.move(group, ElementPlacement.PLACEATBEGINNING);
    whole.move(group, ElementPlacement.PLACEATBEGINNING);
    lit.zOrder(ZOrderMethod.BRINGTOFRONT);
    shade.zOrder(ZOrderMethod.BRINGTOFRONT);
    group.rotate(angle, true, true, true, true, Transformation.BOTTOM);
}

function drawBrimstone(c) {
    paint(smooth(c, [[-76, 64], [-50, 30], [0, 22], [52, 30], [78, 64], [0, 80]]), "#3A3540", OUTLINE, 6);
    crystal(c, -30, 56, 16, 56, 22, "#E6C93A", "#F5E27A", "#A8922E");
    crystal(c, 30, 56, 14, 48, -24, "#E6C93A", "#F5E27A", "#A8922E");
    crystal(c, 0, 58, 20, 90, 0, "#E6C93A", "#F5E27A", "#A8922E");
    sparkle(c, -6, -52, 9);
}

function drawMountainOre(c) {
    var rock = [[-66, 30], [-54, -34], [-14, -66], [40, -58], [70, -10], [60, 46], [4, 70], [-44, 62]];
    paint(shape(c, rock), "#5C5A63", OUTLINE, 8);
    paint(shape(c, [[-66, 30], [-54, -34], [-14, -66], [-6, -20], [-30, 10]]), "#8C8A90", null);
    paint(shape(c, [[70, -10], [60, 46], [4, 70], [16, 10]]), "#3E3C46", null);
    var veins = [[[-40, -10], [-10, 0], [6, -30]], [[-20, 30], [14, 20], [40, 34]], [[10, -40], [36, -24]]];
    for (var i = 0; i < veins.length; i++) line(c, veins[i], "#A9C1D9", 5);
    var specks = [[-28, -36, 6], [24, -10, 7], [-4, 44, 5], [44, 8, 4]];
    for (var k = 0; k < specks.length; k++) paint(diamond(c, specks[k][0], specks[k][1], specks[k][2]), "#E6F0FF", OUTLINE, 2.5);
}

function drawThunderstone(c) {
    glow(c, 0, -10, 70, "#D8E4FF");
    paint(shape(c, [[-22, -80], [22, -80], [30, 40], [0, 92], [-30, 40]]), "#5C5A63", OUTLINE, 8);
    paint(shape(c, [[-22, -80], [-4, -80], [-8, 46], [0, 92], [-30, 40]]), "#8C8A90", null);
    paint(shape(c, [[4, -80], [22, -80], [30, 40], [0, 92], [10, 40]]), "#3E3C46", null);
    paint(oval(c, 0, -80, 22, 8), "#8C8A90", OUTLINE, 5);
    paint(shape(c, [[14, -70], [-12, -14], [6, -10], [-16, 52], [22, -20], [4, -24], [24, -70]]), "#D8E4FF", OUTLINE, 5);
    paint(shape(c, [[14, -66], [-4, -22], [4, -20]]), "#FFFFFF", null, 0, 80);
}

// ---------- Old Forest ----------

function drawYewWood(c) {
    var log = roundRect(c, -6, 14, 150, 64, 30);
    paint(log, "#4A3626", OUTLINE, 7);
    log.rotate(24);
    var bark = roundRect(c, -12, 4, 120, 12, 6);
    paint(bark, "#6A4C34", null, 0, 80);
    bark.rotate(24);
    paint(oval(c, 58, 44, 30, 34), "#C98F5A", OUTLINE, 6);
    paint(oval(c, 58, 44, 19, 22), null, "#9C5E34", 3);
    paint(oval(c, 58, 44, 9, 11), "#9C5E34", null);
    line(c, [[-56, -26], [-20, -60], [20, -76]], "#3E5A2E", 5, true);
    for (var i = 0; i < 6; i++) {
        var t = i / 5;
        var x = -50 + t * 64, y = -32 - t * 40;
        leaf(c, x - 6, y - 10, 4, 12, 30, "#3E5A2E");
        leaf(c, x + 8, y + 4, 4, 12, -50, "#4E6A38");
    }
    paint(circle(c, 22, -72, 8), "#C8362E", OUTLINE, 3.5);
    paint(circle(c, 20, -74, 3), "#F0A090", null);
}

function drawTinderFungus(c) {
    paint(roundRect(c, -60, 10, 40, 180, 10), "#262019", OUTLINE, 6);
    paint(roundRect(c, -66, 10, 6, 160, 3), "#3F3426", null);
    var tiers = [[-4, -40, 64, 26], [0, 4, 76, 30], [-6, 50, 58, 24]];
    for (var i = 0; i < tiers.length; i++) {
        var x = tiers[i][0], y = tiers[i][1], w = tiers[i][2], h = tiers[i][3];
        paint(smooth(c, [[-44, y - h * 0.6], [x + w * 0.4, y - h], [x + w, y - h * 0.2], [x + w * 0.9, y + h * 0.5], [x + w * 0.2, y + h * 0.6], [-44, y + h * 0.3]]), "#6B5440", OUTLINE, 6);
        paint(smooth(c, [[-40, y - h * 0.4], [x + w * 0.4, y - h * 0.78], [x + w * 0.86, y - h * 0.1], [x + w * 0.4, y - h * 0.2]]), "#8E7458", null);
        paint(smooth(c, [[x + w * 0.2, y + h * 0.36], [x + w * 0.86, y + h * 0.3], [x + w * 0.2, y + h * 0.56]], false), null, "#C9B490", 5);
    }
}

function drawOldOakHeartwood(c) {
    paint(circle(c, 0, 0, 82), "#3F3426", OUTLINE, 7);
    paint(circle(c, 0, 0, 70), "#B07A45", OUTLINE, 4);
    paint(circle(c, -8, -8, 60), "#C9925A", null, 0, 70);
    for (var r = 54; r > 10; r -= 12) paint(circle(c, 2, 2, r), null, "#8A5A30", 3);
    glow(c, 2, 2, 22, "#7CE070");
    paint(circle(c, 2, 2, 8), "#C8F5B8", OUTLINE, 3);
}

// ---------- Nav ----------

function bone(c, x, y, length, angle) {
    var g = c.layer.groupItems.add();
    var parts = [roundRect(c, x, y, length, 12, 6), circle(c, x - length / 2, y - 6, 8), circle(c, x - length / 2, y + 6, 8), circle(c, x + length / 2, y - 6, 8), circle(c, x + length / 2, y + 6, 8)];
    for (var i = 0; i < parts.length; i++) {
        paint(parts[i], "#CFCBC0", OUTLINE, 4);
        parts[i].move(g, ElementPlacement.PLACEATEND);
    }
    g.rotate(angle);
}

function drawBoneAsh(c) {
    bone(c, -20, 0, 70, 35);
    bone(c, 28, -10, 56, -40);
    paint(smooth(c, [[-80, 70], [-54, 30], [-20, 14], [20, 10], [56, 28], [82, 70]]), "#8A8A8E", OUTLINE, 6);
    paint(smooth(c, [[-60, 50], [-36, 28], [0, 18], [30, 24], [10, 34], [-30, 40]]), "#B5B5B8", null);
    var flecks = [[-34, 50], [-6, 40], [24, 52], [48, 46], [-50, 62]];
    for (var i = 0; i < flecks.length; i++) paint(circle(c, flecks[i][0], flecks[i][1], 3), "#5A5A60", null);
}

function flame(c, x, y, s) {
    paint(smooth(c, [[x, y - 34 * s], [x + 14 * s, y - 6 * s], [x + 10 * s, y + 12 * s], [x - 10 * s, y + 12 * s], [x - 14 * s, y - 6 * s]]), "#FFB347", OUTLINE, 4);
    paint(smooth(c, [[x, y - 18 * s], [x + 7 * s, y + 2 * s], [x, y + 10 * s], [x - 7 * s, y + 2 * s]]), "#FFF1B0", null);
}

function drawGromnicaWax(c) {
    glow(c, 0, -60, 46, "#FFD45A");
    paint(roundRect(c, 0, 24, 44, 128, 8), "#EDE3C2", OUTLINE, 7);
    paint(roundRect(c, -12, 24, 10, 118, 5), "#FFF8E4", null, 0, 80);
    paint(roundRect(c, 14, 24, 10, 118, 5), "#C9BE9E", null, 0, 70);
    paint(smooth(c, [[-22, -36], [-18, -20], [-12, -34], [-4, -10], [4, -32], [12, -24], [22, -36], [22, -40], [-22, -40]]), "#F8F0D8", OUTLINE, 4);
    paint(roundRect(c, 0, 30, 46, 14, 4), "#4E6E9E", OUTLINE, 4);
    paint(shape(c, [[0, 36], [-14, 64], [-4, 60], [0, 44], [4, 60], [14, 64]]), "#4E6E9E", OUTLINE, 3.5);
    line(c, [[0, -40], [0, -48]], "#3A2E26", 3);
    flame(c, 0, -60, 1.1);
}

function drawNavEssence(c) {
    glow(c, 0, 0, 80, "#B8E8FF");
    paint(smooth(c, [[0, -84], [30, -40], [44, 10], [30, 56], [0, 74], [-30, 56], [-44, 10], [-24, -30], [-10, -10], [-4, -50]]), "#6FA9C9", OUTLINE, 6);
    paint(smooth(c, [[2, -54], [22, -20], [28, 18], [12, 48], [-12, 48], [-26, 18], [-14, -6], [-4, 4]]), "#B8E8FF", null);
    paint(smooth(c, [[2, -14], [14, 14], [4, 34], [-10, 26], [-8, 4]]), "#FFFFFF", null, 0, 90);
    var orbit = [[-58, -40, 7], [56, -30, 5], [-50, 52, 5], [52, 50, 7]];
    for (var i = 0; i < orbit.length; i++) paint(circle(c, orbit[i][0], orbit[i][1], orbit[i][2]), "#B8E8FF", OUTLINE, 3);
}

// ---------- Epic ----------

function drawFirebirdFeather(c) {
    glow(c, 6, -20, 74, "#FFB347");
    var vane = smooth(c, [[40, -86], [58, -48], [46, 4], [12, 48], [-24, 80], [-30, 70], [-14, 30], [0, -20], [18, -64]]);
    paint(vane, "#D9402A", OUTLINE, 7);
    paint(smooth(c, [[38, -78], [50, -46], [38, -4], [10, 36], [-14, 58], [2, 16], [16, -30]]), "#F28A2E", null);
    paint(smooth(c, [[36, -70], [42, -46], [30, -16], [18, -2], [22, -36]]), "#FFD45A", null);
    paint(oval(c, 30, -48, 13, 17), "#FFD45A", OUTLINE, 4);
    paint(oval(c, 30, -46, 6, 8), "#B0402F", null);
    line(c, [[-34, 92], [-22, 70], [4, 28], [26, -20], [40, -82]], "#F5E27A", 3, true);
    sparkle(c, -30, -40, 10);
}

function bottle(c, liquid, light, glass) {
    paint(circle(c, 0, 24, 62), glass, OUTLINE, 7, 60);
    paint(smooth(c, [[-58, 34], [-30, 20], [0, 26], [30, 20], [58, 34], [40, 70], [0, 86], [-40, 70]]), liquid, null);
    paint(roundRect(c, 0, -46, 34, 34, 6), glass, OUTLINE, 6, 60);
    paint(roundRect(c, 0, -70, 42, 18, 6), "#8A6A48", OUTLINE, 5);
    paint(smooth(c, [[-40, 8], [-30, -10], [-16, -18]], false), null, light, 7, 70);
}

function drawDeadWater(c) {
    bottle(c, "#3E5A66", "#C9D6DC", "#7C8E96");
    paint(circle(c, -10, 50, 6), "#6E8A96", null, 0, 80);
    paint(circle(c, 14, 62, 4), "#6E8A96", null, 0, 80);
    paint(roundRect(c, 0, -70, 42, 18, 6), "#A9B4BA", OUTLINE, 5);
}

function drawSerpentKingsCrown(c) {
    glow(c, 0, -6, 70, "#E8B93A");
    var crown = [[-64, 40], [-70, -30], [-40, 4], [-20, -46], [0, -6], [20, -46], [40, 4], [70, -30], [64, 40]];
    paint(shape(c, crown), "#E8B93A", OUTLINE, 7);
    paint(shape(c, [[-64, 40], [-70, -30], [-40, 4], [-20, -46], [-10, 20]]), "#F7D97A", null, 0, 80);
    paint(roundRect(c, 0, 46, 132, 24, 8), "#C9962A", OUTLINE, 6);
    line(c, [[-56, 46], [-36, 38], [-16, 54], [6, 38], [26, 54], [46, 40], [60, 46]], "#4E8A3A", 5, true);
    paint(oval(c, 62, 44, 9, 7), "#4E8A3A", OUTLINE, 3);
    var gems = [[-20, -46], [20, -46], [-70, -30], [70, -30]];
    for (var i = 0; i < gems.length; i++) paint(circle(c, gems[i][0], gems[i][1], 8), "#3FB37A", OUTLINE, 3.5);
    paint(diamond(c, 0, 8, 14), "#3FB37A", OUTLINE, 4);
}

function drawKupalaDew(c) {
    paint(smooth(c, [[-70, 60], [-40, 0], [10, -50], [70, -70], [50, -10], [0, 40]]), "#4E7A3A", OUTLINE, 7);
    paint(smooth(c, [[-62, 52], [-32, 0], [14, -44], [60, -62], [10, -12]]), "#6E9A4A", null);
    line(c, [[-70, 60], [-20, 10], [30, -36], [66, -66]], "#3A5A2A", 3, true);
    var drops = [[-24, 4, 18], [22, -30, 13], [40, -2, 9]];
    for (var i = 0; i < drops.length; i++) {
        var x = drops[i][0], y = drops[i][1], r = drops[i][2];
        paint(circle(c, x, y, r), "#A8E4F0", OUTLINE, 4, 90);
        paint(circle(c, x - r * 0.35, y - r * 0.35, r * 0.3), "#FFFFFF", null);
    }
    sparkle(c, -50, -50, 12);
    sparkle(c, 56, 40, 9);
}

// ---------- Legendary ----------

function drawFernFlower(c) {
    var fronds = [[[-6, 86], [-62, 10], -1], [[6, 86], [62, 10], 1]];
    for (var f = 0; f < fronds.length; f++) {
        var root = fronds[f][0], tip = fronds[f][1], side = fronds[f][2];
        line(c, [root, [(root[0] + tip[0]) / 2 + side * 10, (root[1] + tip[1]) / 2], tip], "#4E5E33", 6, true);
        for (var k = 1; k <= 5; k++) {
            var t = k / 6;
            var x = root[0] + (tip[0] - root[0]) * t, y = root[1] + (tip[1] - root[1]) * t;
            var size = 16 * (1 - t * 0.6);
            leaf(c, x + side * size * 0.9, y, size, size * 0.42, side * 25, "#6E8A3A");
            leaf(c, x - side * size * 0.9, y, size, size * 0.42, -side * 25, "#5B7333");
        }
    }
    line(c, [[0, 88], [0, 0]], "#4E5E33", 7);
    glow(c, 0, -22, 70, "#FFD45A");
    paint(star(c, 0, -22, 56, 22, 6), "#E8552A", OUTLINE, 6);
    var inner = paint(star(c, 0, -22, 34, 14, 6), "#FFB347", null);
    inner.rotate(30);
    paint(circle(c, 0, -22, 12), "#FFF1B0", OUTLINE, 4);
    sparkle(c, 44, -70, 10);
}

function drawLivingWater(c) {
    glow(c, 0, 10, 80, "#5FE0D0");
    bottle(c, "#2FC7B8", "#E6FFFB", "#9FE8E0");
    paint(roundRect(c, 0, -70, 42, 18, 6), "#E8B93A", OUTLINE, 5);
    sparkle(c, -16, 50, 10);
    sparkle(c, 22, 36, 7);
    sparkle(c, 52, -40, 11);
}

function drawAlatyrStone(c) {
    glow(c, 0, 4, 84, "#FFF1C2");
    var stone = [[-60, 40], [-70, -10], [-40, -60], [10, -72], [58, -46], [72, 6], [50, 56], [0, 72]];
    paint(smooth(c, stone), "#E9E4D6", OUTLINE, 7);
    paint(smooth(c, [[-56, 30], [-64, -10], [-36, -54], [6, -64], [-10, -20], [-30, 10]]), "#FFFFFF", null, 0, 80);
    paint(smooth(c, [[60, 4], [44, 50], [0, 64], [20, 30], [40, 0]]), "#B9B3A6", null, 0, 80);
    paint(star(c, 0, 0, 34, 12, 8), null, "#9A968C", 4);
    paint(circle(c, 0, 0, 18), "#F7F1E0", "#9A968C", 3);
    paint(circle(c, 0, 0, 8), "#F2C230", OUTLINE, 3);
    sparkle(c, 46, -58, 11);
}
