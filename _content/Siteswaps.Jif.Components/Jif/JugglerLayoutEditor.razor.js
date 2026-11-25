let activeDrag;

function svgPoint(svg, clientX, clientY) {
    const point = svg.createSVGPoint();
    point.x = clientX;
    point.y = clientY;
    const matrix = svg.getScreenCTM();
    if (!matrix) return null;
    return point.matrixTransform(matrix.inverse());
}

function begin(svg, dotnet, person, pointerId, onMove) {
    stop();
    const move = event => {
        if (event.pointerId !== pointerId) return;
        const point = svgPoint(svg, event.clientX, event.clientY);
        if (!point) return;
        onMove(point);
        event.preventDefault();
    };
    const finish = event => {
        if (event.pointerId !== pointerId) return;
        stop();
        dotnet.invokeMethodAsync("FinishDrag");
    };
    activeDrag = () => {
        document.removeEventListener("pointermove", move);
        document.removeEventListener("pointerup", finish, true);
        document.removeEventListener("pointercancel", finish);
        activeDrag = undefined;
    };
    document.addEventListener("pointermove", move, { passive: false });
    document.addEventListener("pointerup", finish, true);
    document.addEventListener("pointercancel", finish);
}

export function startMove(svg, dotnet, person, pointerId) {
    begin(svg, dotnet, person, pointerId, point => {
        dotnet.invokeMethodAsync("MovePerson", person, (point.x - 180) / 145, (point.y - 120) / 85);
    });
}

export function startRotate(svg, dotnet, person, pointerId, centerX, centerY) {
    begin(svg, dotnet, person, pointerId, point => {
        const degrees = Math.atan2(point.x - centerX, centerY - point.y) * 180 / Math.PI;
        dotnet.invokeMethodAsync("RotatePersonTo", person, (degrees + 360) % 360);
    });
}

export function stop() {
    if (activeDrag) activeDrag();
}

export function showDialog(dialog) {
    if (!dialog.open) dialog.showModal();
}

export function closeDialog(dialog) {
    if (dialog.open) dialog.close();
}
