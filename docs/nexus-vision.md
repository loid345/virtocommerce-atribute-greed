# Nexus: Isomorphic Rust Platform

## Vision
Nexus is an open-source platform that makes building web products feel like shipping a Linux distro for the web: fast, free, and composable. The goal is to make paid platforms like Salesforce, Shopify, or Wix feel obsolete by delivering a unified Rust experience across backend, frontend, data, and jobs.

## Core Thesis
One language. One module. One mental model.

Rust becomes the full-stack runtime via **Axum** (backend) and **Leptos** (frontend + SSR). Data types are shared across the stack, enabling a single source of truth for validation and eliminating class-of-bugs from mismatched contracts.

## Architecture: Isomorphic Monolith
We abandon the outdated split between “frontend” and “backend.” Instead, each module encapsulates the entire product slice:

- **Database**: SeaORM migrations and entities.
- **API**: Axum handlers and service logic.
- **UI**: Leptos components for pages, forms, and admin.
- **Jobs**: background tasks like email, imports, or ETL.

When a module is added in `Cargo.toml`, it brings its database schema, API, UI, and jobs—fully wired.

## Stack: Maximum Leverage
We build on the strongest Rust ecosystem primitives out of the box:

- **Server**: Axum
- **UI (Client + SSR)**: Leptos
- **ORM**: SeaORM
- **Styling**: TailwindCSS (generated via Leptos macros)
- **State**: Leptos signals (no Redux)
- **RPC**: Leptos Server Functions (call server functions directly from UI)

## The Nexus CLI
Rust is powerful but slow to bootstrap. We solve this with a first-class CLI generator.

```bash
# 1. Create a project
nexus new my-empire

# 2. Add modules (crates)
nexus add auth        # Users, OAuth, 2FA, JWT
nexus add commerce    # Products, cart, payments
nexus add blog        # Posts, SEO, comments
nexus add crm         # Leads, funnels, chats
nexus add ai          # LLM integration

# 3. Run
nexus run
```

The CLI fetches modules, generates a unified `main.rs`, assembles the full Leptos app, and compiles the monolith.

## Why Free Works (AGPLv3)
- Open source codebase.
- No license fee.
- Ecosystem wins: the platform grows because building modules is 10x faster than building from scratch.

We aim to become the Rust-native standard for business platforms—like WordPress, but fast, safe, and modular.

## Universal Traits
To keep modules provider-agnostic, Nexus defines core traits:

- `PaymentProvider`: Stripe, crypto, cash, anything.
- `FileStorage`: S3, MinIO, local disk, IPFS.
- `Notification`: Email, SMS, Slack, Telegram, Push.

## Example: Leptos + Server Function
The same Rust code powers server logic and UI:

```rust
// in nexus_commerce module

#[server(BuyProduct, "/api")]
pub async fn buy_product(id: Uuid) -> Result<String, ServerFnError> {
    let db = use_context::<Database>().unwrap();
    db.orders.insert(id).await?;
    Ok("Куплено!".to_string())
}

#[component]
pub fn BuyButton(id: Uuid) -> impl IntoView {
    let buy_action = create_server_action::<BuyProduct>();

    view! {
        <button
            class="btn-primary"
            on:click=move |_| buy_action.dispatch(BuyProduct { id })
        >
            "Купить сейчас"
        </button>

        {move || buy_action.value().get().map(|res| view! { <span>{res}</span> })}
    }
}
```

## Next Steps
- Define the minimal module interface (DB + API + UI + Jobs).
- Scaffold the CLI with `nexus new/add/run` commands.
- Create the first reference module (auth + admin UI).
