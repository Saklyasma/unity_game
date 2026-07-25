/** Plain-texts a Markdown reply before feeding it to speechSynthesis. */
export function stripMarkdown(text) {
  return text
    .replace(/```[\s\S]*?```/g, '')
    .replace(/`([^`]+)`/g, '$1')
    .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
    .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
    .replace(/[*_~#>-]/g, '')
    .replace(/\s+/g, ' ')
    .trim()
}
