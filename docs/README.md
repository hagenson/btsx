# BTSX Documentation

This directory contains the Hugo-based documentation site for BTSX.

## Setup

### Prerequisites

- [Hugo Extended](https://gohugo.io/installation/) version 0.112.0 or later

### Install Hugo Book Theme

The Hugo Book theme needs to be installed as a Git submodule:

```bash
git submodule add https://github.com/alex-shpak/hugo-book docs/themes/hugo-book
git submodule update --init --recursive
```

## Development

### Run Local Server

To preview the documentation site locally:

```bash
cd docs
hugo server -D
```

The site will be available at `http://localhost:1313/help/`

### Build for Production

To build the static site:

```bash
cd docs
hugo
```

The built site will be in the `docs/public/` directory.

## Structure

- `content/` - Documentation content in Markdown format
- `static/` - Static assets (images, CSS, JS)
- `themes/hugo-book/` - Hugo Book theme (Git submodule)
- `hugo.toml` - Site configuration
- `archetypes/` - Content templates

## Configuration

The site is configured via `hugo.toml` with:

- Base URL: `/help/`
- Theme: Hugo Book
- Search enabled
- Table of contents enabled
- GitHub integration

## Adding Content

Create new documentation pages:

```bash
hugo new content/page-name.md
```

Or manually create Markdown files in the `content/` directory.
