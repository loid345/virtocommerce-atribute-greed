# Attribute Grid

## Overview

The Attribute Grid module provides a clean, compact manager for product attributes in Virto Commerce.
The UI focuses on minimal visual noise by replacing bulky trees with compact dropdowns and breadcrumbs.

- **New experience:** A grid-first, minimal UI for attribute management with inline actions and compact filters.
- **Replacement:** Replaces legacy attribute screens with a single grid and a compact edit form.
- **Dependencies:** Catalog for catalog/category metadata and platform UI (blades, permissions, settings).
- **Deployment:** Admins use it to manage product properties in large catalogs while keeping the UI lightweight.

## Functional Requirements

- Grid with search, catalog filter, and type filter.
- Attribute rows show name, code, type, scope, owner path, filter flag, and usage.
- Owner selection is represented as dropdown-based catalog/category selection and a breadcrumb path string.
- Actions are lightweight (inline edit/delete) and trash is a separate entry point.

## Scenarios

List of scenarios that the new module implements:

1. Search and filter attributes in a minimal grid.
2. Inspect attribute ownership via breadcrumb-like path.
3. Create or edit attributes from a compact form.
4. Manage deleted attributes in a separate trash view.

## Web API

Web API documentation for each module is built out automatically and can be accessed by following the link bellow:
<https://link-to-swager-api>

## Database Model

![DB model](./docs/media/diagram-db-model.png)

## Related topics

[Some Article1](some-article1.md)

[Some Article2](some-article2.md)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

<https://virtocommerce.com/open-source-license>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
