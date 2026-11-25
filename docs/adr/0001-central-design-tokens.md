# Central design option and decision tokens
Date: 2026-11-24
Status: Accepted

## Why
Generator and Causal Diagram styles define the same design values independently, which causes duplicated tokens and visual drift.

## Decision
Define design options and semantic design decisions centrally, without a `ds` prefix; Generator and Causal Diagram consume the same decision tokens, and component code selects semantic decisions rather than defining design values.

## What we did not choose
We did not keep `wizard-*`, `sd-*`, or feature-specific token families, because they duplicate the same design system and permit those features to diverge.
