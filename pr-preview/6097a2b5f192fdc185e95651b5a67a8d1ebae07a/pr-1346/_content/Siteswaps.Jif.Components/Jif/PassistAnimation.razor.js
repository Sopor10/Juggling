import AnimationWidget from 'https://passist.org/api/animation-widget-standalone.mjs';

const widgets = new WeakMap();

export function render(target, jifJson, animationSpeed, teaser = false, enableSettings = true) {
    const jif = normalizeForPassist(JSON.parse(jifJson));
    widgets.get(target)?.$destroy?.();
    target.replaceChildren();
    widgets.set(target, new AnimationWidget({
        target,
        props: { jif, animationSpeed, teaser, enableSettings },
    }));
}

export function update(target, animationSpeed) {
    widgets.get(target)?.$set({ animationSpeed });
}

function normalizeForPassist(jif) {
    // jif-web calls these objects; Passist's renderer calls them props.
    if (!jif.props && jif.objects) {
        jif.props = jif.objects;
    }

    const jugglers = jif.jugglers ?? [];
    for (const [index, juggler] of jugglers.entries()) {
        juggler.name ??= juggler.label ?? String.fromCodePoint(65 + index);

        if (!juggler.position) {
            if (jugglers.length === 1) {
                juggler.position = [0, 0, 0];
            } else {
                const radius = 1.2 + jugglers.length * 0.2;
                const angle = Math.PI * 2 * index / jugglers.length;
                const round = value => Math.round(value * 1000) / 1000;
                juggler.position = [round(radius * Math.cos(angle)), 0, round(radius * Math.sin(angle))];
            }
        }

        juggler.lookAt ??= jugglers.length === 1 ? [0, 0, 1] : [0, 0, 0];
    }

    for (const limb of jif.limbs ?? []) {
        if (!limb.type && limb.kind) {
            limb.type = ({
                right_hand: 'right hand',
                left_hand: 'left hand',
                other: 'other',
            })[limb.kind] ?? limb.kind.replaceAll('_', ' ');
        }
    }

    return jif;
}

export function destroy(target) {
    widgets.get(target)?.$destroy?.();
    widgets.delete(target);
}
