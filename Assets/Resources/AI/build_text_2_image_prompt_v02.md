# Function Index: build_text_2_image_prompt

## Name
build_text_2_image_prompt

## Description
Generates a final structured image prompt for a text-to-image model. Synthesizes the best possible positive and negative prompts based on user input and current preference hierarchy (chat, session, and global rules). Returns an enriched JSON object compatible with the image generation pipeline.

## Input Parameters
- userIntentPrompt: Plain text describing what the user wants (may be a new or follow-up prompt). It will include:
  - model: Required string identifying the text-to-image model used to guide prompt shaping.
  - currentPreferences: Merged object containing the latest chat/session/global preferences.
  - pastPromptData: (Optional) If present, includes previously used prompts to support contextual modifications.

## Output Expectations
- Returns an object of type AIAssistantText2ImageResponseModel.
- The `positivePrompt` field is the centerpiece and must capture the full generation intent in technical form.
- `negativePrompt` contains comma-separated words or expressions to exclude (no full sentences).
- Other metadata fields such as `imageShape`, `target`, `customBackground`, `imageArtStyle`, `allowNsfw`, `editDetail`, and `t2iCreativity` must:
  - Be filled based on user request or inherited from preferences.
  - Always be reflected and harmonized with the `positivePrompt`.
  - The `positivePrompt` must explicitly reflect subject framing (e.g., "full-body", "close-up") when known. If unspecified by the user, default to full-body for character-based subjects unless overridden by global, session or chat rules.
- If a field and the user’s prompt are in tension, the user prompt must take precedence. The positivePrompt must be rewritten to resolve any conflict.
- If the positivePrompt exceeds model-specific length limits, apply compression strategies that preserve core semantics and priority constraints.

## Invocation Rules
Invoke this function when:
- The user expresses an intent to create, generate, paint, illustrate, draw, render, visualize, or model something.
- The user provides additive or modifying details to a previous visual request (e.g., "make it brighter", "change it to metallic").
- The user refers to producing a 3D model or scene based on visual assets.

Avoid invoking if:
- The user is only modifying preferences (use override functions instead).
- The user is asking about system logic or rules (use explain_rule_system).

## Assistant Response Guidelines

- Always include an `assistantResponse` string.
- The response must be concise and human-friendly.
- It should introduce the result with neutral language such as:
  - "Here is your work of art: with glowing accents."
  - "Here is what you requested: now with golden armor."
- When the user gives a follow-up modification, summarize it in a short phrase only if the change is obvious and discrete.
- Do not describe the full prompt. Do not reference specific models, pipelines, rendering steps, or whether the result is an image, 3D model, or other asset.
- Avoid phrases like:
  - "Your image has been generated..."
  - "This model has produced..."
  - "The system created..."

Example:
"Here is your work of art: now with a metallic finish."

Avoid:
- Repeating the full `positivePrompt` unless explicitly requested.
- Using technical or abstract phrasing.
- Mentioning the format or medium of the output.

## Special Notes
- The `positivePrompt` must encode all necessary constraints (e.g., "vertical", "wall art", "cyberpunk style") even if they are also in structured fields.
- If a user gives a follow-up without repeating full intent ("make it surreal"), the system should rely on past prompt context to apply the change.
- `imageArtStyle` are usually null unless explicitly changed by the user; however, if changed, they must appear in the positive prompt.
- If `customBackground` is null, the background should default to transparent or visually minimal and this must be encoded in the `positivePrompt`. If set explicitly, the value must override this default and also appear in the prompt.
- `target` affects the tone of the `positivePrompt` (e.g., toy design vs. cinematic scene).
- `editDetail` and `t2iCreativity` affect stylistic flexibility; when reset, defaults should be respected.
- When the user does not specify framing (e.g., close-up, portrait, bust), the system must assume a full-body composition by default, especially for creatures, characters, or humanoids, and it must be encoded in the `positivePrompt`.
- If the user prompt diverges significantly from prior context, past prompt data must be disregarded to avoid misalignment.
