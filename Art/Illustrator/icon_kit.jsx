// Shared by the icon scripts: colours, rarity frames, drawing helpers and the artboard and PNG export. An entry without a rarity gets no frame.

var SIZE = 256;
var GAP = 64;

var OUTLINE = "#120D0F";
var PLATE = "#2B2420";
var PLATE_SHADE = "#1E1915";
var RARITY = {
    common: { rim: "#9C9C9C", light: "#D2D2D2" },
    uncommon: { rim: "#5FB04A", light: "#A6E08C" },
    rare: { rim: "#3F86E0", light: "#9CC6FF" },
    epic: { rim: "#9B4FE0", light: "#D2A4FF" },
    legendary: { rim: "#F2C230", light: "#FFE9A0" }
};


// Draws every entry ([name, rarity, draw function]) on its own artboard and layer, row by row, and exports one PNG per artboard to the out folder.
function buildIcons(rows, out) {
    new Folder(out).create();
    app.userInteractionLevel = UserInteractionLevel.DONTDISPLAYALERTS;
    var doc = app.documents.add(DocumentColorSpace.RGB, SIZE, SIZE);
    app.coordinateSystem = CoordinateSystem.DOCUMENTCOORDINATESYSTEM;
    var first = doc.artboards[0].artboardRect;
    var names = [];

    for (var r = 0; r < rows.length; r++) {
        for (var k = 0; k < rows[r].length; k++) {
            var entry = rows[r][k];
            var left = first[0] + k * (SIZE + GAP);
            var top = first[1] - r * (SIZE + GAP);
            var rect = [left, top, left + SIZE, top - SIZE];
            var board = names.length == 0 ? doc.artboards[0] : doc.artboards.add(rect);
            board.artboardRect = rect;
            board.name = entry[0];
            var layer = doc.layers.add();
            layer.name = entry[0];
            var c = { x: (rect[0] + rect[2]) / 2, y: (rect[1] + rect[3]) / 2, layer: layer };
            if (entry[1]) drawFrame(c, entry[1]);
            if (entry[2]) entry[2](c);
            names.push(entry[0]);
        }
    }

    for (var j = 0; j < names.length; j++) {
        doc.artboards.setActiveArtboardIndex(j);
        var png = new ExportOptionsPNG24();
        png.artBoardClipping = true;
        png.transparency = true;
        png.antiAliasing = true;
        doc.exportFile(new File(out + names[j] + ".png"), ExportType.PNG24, png);
    }
    return doc;
}

// ---------- helpers: local coordinates have y pointing down, (0,0) is the icon centre ----------

function rgb(hex) {
    var color = new RGBColor();
    color.red = parseInt(hex.substr(1, 2), 16);
    color.green = parseInt(hex.substr(3, 2), 16);
    color.blue = parseInt(hex.substr(5, 2), 16);
    return color;
}

function paint(item, fill, stroke, width, opacity) {
    if (fill) { item.filled = true; item.fillColor = rgb(fill); } else item.filled = false;
    if (stroke) {
        item.stroked = true;
        item.strokeColor = rgb(stroke);
        item.strokeWidth = width;
        item.strokeJoin = StrokeJoin.ROUNDENDJOIN;
        item.strokeCap = StrokeCap.ROUNDENDCAP;
    } else item.stroked = false;
    if (opacity !== undefined) item.opacity = opacity;
    return item;
}

function pt(c, x, y) { return [c.x + x, c.y - y]; }

function shape(c, points, closed) {
    var path = c.layer.pathItems.add();
    var abs = [];
    for (var i = 0; i < points.length; i++) abs.push(pt(c, points[i][0], points[i][1]));
    path.setEntirePath(abs);
    path.closed = closed !== false;
    return path;
}

// Catmull-Rom style handles, so organic shapes get smooth curves
function smooth(c, points, closed) {
    var isClosed = closed !== false;
    var path = shape(c, points, isClosed);
    var n = points.length;
    for (var i = 0; i < n; i++) {
        var prev = points[isClosed ? (i - 1 + n) % n : Math.max(i - 1, 0)];
        var next = points[isClosed ? (i + 1) % n : Math.min(i + 1, n - 1)];
        var dx = (next[0] - prev[0]) / 6, dy = (next[1] - prev[1]) / 6;
        var p = path.pathPoints[i];
        p.leftDirection = pt(c, points[i][0] - dx, points[i][1] - dy);
        p.rightDirection = pt(c, points[i][0] + dx, points[i][1] + dy);
        p.pointType = PointType.SMOOTH;
    }
    return path;
}

function circle(c, x, y, r) { return c.layer.pathItems.ellipse(c.y - y + r, c.x + x - r, r * 2, r * 2); }
function oval(c, x, y, rx, ry) { return c.layer.pathItems.ellipse(c.y - y + ry, c.x + x - rx, rx * 2, ry * 2); }
function star(c, x, y, r, inner, points) { return c.layer.pathItems.star(c.x + x, c.y - y, r, inner, points); }
function polygon(c, x, y, r, sides) { return c.layer.pathItems.polygon(c.x + x, c.y - y, r, sides); }
function roundRect(c, x, y, w, h, radius) { return c.layer.pathItems.roundedRectangle(c.y - y + h / 2, c.x + x - w / 2, w, h, radius, radius); }
function diamond(c, x, y, r) { return shape(c, [[x, y - r], [x + r, y], [x, y + r], [x - r, y]]); }

function line(c, points, color, width, curved) {
    paint(curved ? smooth(c, points, false) : shape(c, points, false), null, OUTLINE, width + 9);
    return paint(curved ? smooth(c, points, false) : shape(c, points, false), null, color, width);
}

function glow(c, x, y, r, color) {
    paint(circle(c, x, y, r), color, null, 0, 14);
    paint(circle(c, x, y, r * 0.72), color, null, 0, 22);
}

function sparkle(c, x, y, r) { paint(star(c, x, y, r, r * 0.28, 4), "#FFFFFF", null, 0, 90); }

// ---------- square frames: the colour and the corner ornaments both tell the rarity ----------

function drawFrame(c, rarity) {
    var colors = RARITY[rarity];
    paint(roundRect(c, 0, 0, 246, 246, 40), OUTLINE, null);
    paint(roundRect(c, 0, 0, 236, 236, 34), colors.rim, null);
    paint(roundRect(c, 0, -4, 224, 224, 30), colors.light, null, 0, 45);
    paint(roundRect(c, 0, 0, 216, 216, 28), colors.rim, null);
    paint(roundRect(c, 0, 0, 206, 206, 24), OUTLINE, null);
    paint(roundRect(c, 0, 0, 198, 198, 20), PLATE, null);
    paint(oval(c, 0, 68, 84, 22), PLATE_SHADE, null, 0, 70);

    var corners = [[-101, -101], [101, -101], [101, 101], [-101, 101]];
    for (var i = 0; i < corners.length; i++) {
        var x = corners[i][0], y = corners[i][1];
        if (rarity == "uncommon") paint(circle(c, x, y, 10), colors.light, OUTLINE, 4);
        if (rarity == "rare" || rarity == "epic") paint(diamond(c, x, y, 14), colors.light, OUTLINE, 4);
        if (rarity == "legendary") paint(star(c, x, y, 19, 7, 4), colors.light, OUTLINE, 4);
    }
    if (rarity == "epic") paint(diamond(c, 0, -112, 13), colors.light, OUTLINE, 4);
    if (rarity == "legendary") {
        paint(shape(c, [[-30, -104], [-30, -120], [-15, -110], [0, -124], [15, -110], [30, -120], [30, -104]]), colors.light, OUTLINE, 4);
        paint(circle(c, 0, 118, 9), colors.light, OUTLINE, 4);
    }
}
