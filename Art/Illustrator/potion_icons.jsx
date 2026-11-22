#target illustrator

// Draws every potion icon as editable vectors in a new document and exports 256 px PNGs to Assets/Art/UI/Potions.
// A potion's frame takes the rarity of its rarest ingredient; the bottle shape, liquid colour and label sign tell potions apart.
var OUT = new File($.fileName).parent.parent.parent.fsName.replace(/\\/g, "/") + "/Assets/Art/UI/Potions/";
#include "icon_kit.jsx"

var GLASS = "#9FB3B0";
var LABEL = "#E9DFC4";
var CORK = "#8A6A48";

var ROWS = [
    [["HealingDraught", "common", drawHealingDraught], ["CleansingBrew", "uncommon", drawCleansingBrew], ["WolfsFury", "uncommon", drawWolfsFury], ["SheafWard", "rare", drawSheafWard], ["BogMist", "uncommon", drawBogMist]],
    [["WispOil", "rare", drawWispOil], ["FireFlask", "common", drawFireFlask], ["StormDraught", "rare", drawStormDraught], ["HeartwoodBrew", "rare", drawHeartwoodBrew], ["GromnicaLight", "uncommon", drawGromnicaLight]],
    [["FirebirdDraught", "epic", drawFirebirdDraught], ["DeadWaterTincture", "epic", drawDeadWaterTincture], ["KupalaDewElixir", "epic", drawKupalaDewElixir]],
    [["LivingWaterElixir", "legendary", drawLivingWaterElixir], ["FernFlowerElixir", "legendary", drawFernFlowerElixir]]
];

buildIcons(ROWS, OUT);

// ---------- bottles: glass, liquid, highlight, neck and cork ----------

function neck(c, top, width, cork) {
    paint(roundRect(c, 0, top + 14, width, 30, 6), GLASS, OUTLINE, 6, 70);
    paint(roundRect(c, 0, top - 6, width + 12, 22, 7), cork || CORK, OUTLINE, 5);
    paint(roundRect(c, -6, top - 10, width - 4, 6, 3), "#B08C64", null);
}

function roundFlask(c, liquid, light, cork) {
    neck(c, -66, 30, cork);
    paint(circle(c, 0, 26, 62), GLASS, OUTLINE, 7, 55);
    paint(smooth(c, [[-58, 34], [-30, 16], [0, 22], [30, 14], [58, 32], [44, 68], [0, 86], [-44, 68]]), liquid, null);
    paint(smooth(c, [[-56, 36], [-30, 20], [0, 26], [-20, 40]]), light, null, 0, 70);
    paint(smooth(c, [[-42, 0], [-32, -18], [-16, -26]], false), null, "#FFFFFF", 7, 60);
}

function tallVial(c, liquid, light, cork) {
    neck(c, -74, 28, cork);
    paint(roundRect(c, 0, 20, 76, 150, 30), GLASS, OUTLINE, 7, 55);
    paint(roundRect(c, 0, 42, 62, 100, 24), liquid, null);
    paint(roundRect(c, -14, 34, 14, 80, 7), light, null, 0, 60);
    paint(roundRect(c, -24, 6, 8, 90, 4), "#FFFFFF", null, 0, 55);
}

function squareBottle(c, liquid, light, cork) {
    neck(c, -70, 32, cork);
    paint(roundRect(c, 0, 22, 112, 124, 20), GLASS, OUTLINE, 7, 55);
    paint(roundRect(c, 0, 44, 98, 80, 14), liquid, null);
    paint(roundRect(c, -30, 40, 18, 70, 8), light, null, 0, 60);
    paint(roundRect(c, -40, 10, 8, 90, 4), "#FFFFFF", null, 0, 50);
}

function gourd(c, liquid, light, cork) {
    neck(c, -74, 26, cork);
    paint(smooth(c, [[0, -50], [30, -36], [26, -6], [58, 24], [56, 68], [0, 90], [-56, 68], [-58, 24], [-26, -6], [-30, -36]]), GLASS, OUTLINE, 7, 55);
    paint(smooth(c, [[-52, 34], [-20, 20], [20, 24], [52, 34], [44, 68], [0, 84], [-44, 68]]), liquid, null);
    paint(smooth(c, [[-48, 40], [-20, 28], [-30, 52]]), light, null, 0, 65);
    paint(smooth(c, [[-38, 18], [-20, -2], [-16, -26]], false), null, "#FFFFFF", 6, 55);
}

function label(c, y) {
    paint(circle(c, 0, y, 26), LABEL, OUTLINE, 4);
}

// ---------- signs on the labels ----------

function heart(c, x, y, s, color) {
    paint(smooth(c, [[x, y + 12 * s], [x - 15 * s, y - 2 * s], [x - 12 * s, y - 12 * s], [x - 4 * s, y - 12 * s], [x, y - 6 * s], [x + 4 * s, y - 12 * s], [x + 12 * s, y - 12 * s], [x + 15 * s, y - 2 * s]]), color, OUTLINE, 3);
}

function drop(c, x, y, s, color) {
    paint(smooth(c, [[x, y - 16 * s], [x + 10 * s, y + 2 * s], [x + 8 * s, y + 12 * s], [x - 8 * s, y + 12 * s], [x - 10 * s, y + 2 * s]]), color, OUTLINE, 3);
}

function bolt(c, x, y, s, color) {
    paint(shape(c, [[x + 4 * s, y - 16 * s], [x - 8 * s, y + 2 * s], [x, y + 2 * s], [x - 4 * s, y + 16 * s], [x + 9 * s, y - 3 * s], [x + 1 * s, y - 3 * s]]), color, OUTLINE, 3);
}

function shieldSign(c, x, y, s, color) {
    paint(smooth(c, [[x, y - 15 * s], [x + 13 * s, y - 10 * s], [x + 12 * s, y + 4 * s], [x, y + 16 * s], [x - 12 * s, y + 4 * s], [x - 13 * s, y - 10 * s]]), color, OUTLINE, 3);
}

function oakLeaf(c, x, y, s, color) {
    paint(smooth(c, [[x, y - 16 * s], [x + 8 * s, y - 10 * s], [x + 5 * s, y - 4 * s], [x + 10 * s, y + 2 * s], [x + 4 * s, y + 8 * s], [x, y + 16 * s], [x - 4 * s, y + 8 * s], [x - 10 * s, y + 2 * s], [x - 5 * s, y - 4 * s], [x - 8 * s, y - 10 * s]]), color, OUTLINE, 3);
}

// ---------- potions ----------

function drawHealingDraught(c) {
    roundFlask(c, "#4FC35A", "#9BE88E");
    label(c, 40);
    heart(c, 0, 40, 1, "#C8362E");
}

function drawCleansingBrew(c) {
    tallVial(c, "#9FDCEB", "#E2F7FC");
    label(c, 44);
    paint(star(c, 0, 44, 16, 5, 4), "#4F9FD8", OUTLINE, 3);
    sparkle(c, 30, -20, 9);
}

function drawWolfsFury(c) {
    squareBottle(c, "#B0322A", "#E0705C");
    label(c, 46);
    for (var i = -1; i <= 1; i++) paint(shape(c, [[i * 8 + 4, 30], [i * 8 - 4, 60]], false), null, "#7A1E18", 5);
}

function drawSheafWard(c) {
    roundFlask(c, "#E2B84A", "#F5DC8C");
    label(c, 40);
    shieldSign(c, 0, 40, 1, "#9C9C9C");
    paint(shape(c, [[0, 28], [0, 52]], false), null, "#C9B77A", 3);
}

function drawBogMist(c) {
    gourd(c, "#7D8460", "#A9AE8C");
    label(c, 46);
    paint(circle(c, -8, 48, 8), "#B5B5B8", OUTLINE, 2.5);
    paint(circle(c, 6, 44, 10), "#B5B5B8", OUTLINE, 2.5);
    paint(circle(c, 12, 52, 6), "#B5B5B8", OUTLINE, 2.5);
    var puffs = [[-30, -78, 8], [-24, -94, 6], [28, -82, 9], [36, -66, 6]];
    for (var i = 0; i < puffs.length; i++) paint(circle(c, puffs[i][0], puffs[i][1], puffs[i][2]), "#A9AE8C", OUTLINE, 3, 85);
}

function drawWispOil(c) {
    glow(c, 0, 26, 56, "#8FE0C8");
    roundFlask(c, "#3E8A7A", "#8FE0C8");
    label(c, 40);
    paint(oval(c, 0, 40, 15, 9), "#FFFFFF", OUTLINE, 3);
    paint(circle(c, 0, 40, 5), "#2E7A6A", null);
}

function drawFireFlask(c) {
    roundFlask(c, "#F28A2E", "#FFC266", "#7A5A3A");
    paint(smooth(c, [[-20, -84], [-6, -92], [10, -86], [22, -92], [14, -80], [-14, -78]]), "#D8C9A8", OUTLINE, 4);
    paint(smooth(c, [[3, -112], [14, -97], [10, -87], [-5, -87], [-9, -97]]), "#FFB347", OUTLINE, 4);
    paint(smooth(c, [[3, -103], [8, -95], [3, -90], [-2, -95]]), "#FFF1B0", null);
    label(c, 40);
    paint(smooth(c, [[0, 26], [10, 40], [6, 52], [-6, 52], [-10, 40]]), "#E8552A", OUTLINE, 3);
}

function drawStormDraught(c) {
    tallVial(c, "#5C78D8", "#A9BCF5");
    label(c, 44);
    bolt(c, 0, 44, 1.1, "#FFE14A");
    sparkle(c, 30, -16, 9);
}

function drawHeartwoodBrew(c) {
    squareBottle(c, "#A8642E", "#D8955A");
    label(c, 46);
    oakLeaf(c, 0, 46, 1, "#6E8A3A");
}

function drawGromnicaLight(c) {
    glow(c, 0, 30, 60, "#FFD45A");
    tallVial(c, "#F2D07A", "#FFF1C2");
    label(c, 44);
    paint(roundRect(c, 0, 52, 8, 16, 2), "#EDE3C2", OUTLINE, 2.5);
    drop(c, 0, 36, 0.6, "#FFB347");
}

function drawFirebirdDraught(c) {
    glow(c, 0, 20, 76, "#FFB347");
    roundFlask(c, "#E8552A", "#FFB347");
    label(c, 40);
    paint(smooth(c, [[8, 22], [14, 34], [6, 50], [-8, 58], [-6, 44], [2, 32]]), "#FFD45A", OUTLINE, 3);
    sparkle(c, -38, -34, 10);
    sparkle(c, 44, 0, 8);
}

function drawDeadWaterTincture(c) {
    squareBottle(c, "#3E5A66", "#7C9AA6", "#A9B4BA");
    label(c, 46);
    drop(c, 0, 46, 1, "#A9B4BA");
}

function drawKupalaDewElixir(c) {
    glow(c, 0, 24, 70, "#A8E4F0");
    gourd(c, "#6FCFD8", "#CFF4F8");
    label(c, 46);
    drop(c, -6, 48, 0.7, "#A8E4F0");
    paint(smooth(c, [[4, 56], [10, 40], [18, 34], [14, 48]]), "#6E9A4A", OUTLINE, 2.5);
    sparkle(c, -40, -30, 10);
    sparkle(c, 42, 2, 8);
}

function drawLivingWaterElixir(c) {
    glow(c, 0, 16, 84, "#5FE0D0");
    tallVial(c, "#2FC7B8", "#9FF5EA", "#E8B93A");
    label(c, 44);
    paint(star(c, 0, 44, 17, 8, 8), "#F2C230", OUTLINE, 3);
    sparkle(c, -40, -40, 11);
    sparkle(c, 42, -6, 9);
    sparkle(c, -36, 76, 8);
}

function drawFernFlowerElixir(c) {
    glow(c, 0, 20, 84, "#FFD45A");
    roundFlask(c, "#C0392B", "#F28A2E", "#E8B93A");
    label(c, 40);
    paint(star(c, 0, 40, 18, 7, 6), "#E8552A", OUTLINE, 3);
    paint(circle(c, 0, 40, 5), "#FFF1B0", OUTLINE, 2);
    sparkle(c, -42, -34, 11);
    sparkle(c, 44, 0, 9);
}
