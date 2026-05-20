-- =============================================================================
-- ForeFront — database initialisation script
-- Executed automatically by PostgreSQL on first container start.
-- Re-running is safe: all statements use IF NOT EXISTS.
-- =============================================================================

-- ---------------------------------------------------------------------------
-- customers
-- One row per registered customer.
-- customer_id : application-generated GUID
-- email       : must be unique — used as a login identifier
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS customers (
    customer_id  TEXT  NOT NULL,
    namn         TEXT  NOT NULL,
    email        TEXT  NOT NULL,

    CONSTRAINT pk_customers       PRIMARY KEY (customer_id),
    CONSTRAINT uq_customers_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_customers_email
    ON customers (email);

-- ---------------------------------------------------------------------------
-- products
-- Product catalogue. namn and pris are the authoritative values.
-- order_lines stores a snapshot of them at order time.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS products (
    product_id  TEXT            NOT NULL,
    namn        TEXT            NOT NULL,
    pris        NUMERIC(18, 2)  NOT NULL CHECK (pris > 0),

    CONSTRAINT pk_products PRIMARY KEY (product_id)
);

-- ---------------------------------------------------------------------------
-- orders
-- kund_id references customers.
-- RESTRICT prevents deleting a customer who still has orders.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS orders (
    order_id   TEXT         NOT NULL,
    kund_id    TEXT         NOT NULL,
    status     VARCHAR(20)  NOT NULL DEFAULT 'Pending',
    created    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT pk_orders PRIMARY KEY (order_id),
    CONSTRAINT fk_orders_customer
        FOREIGN KEY (kund_id)
        REFERENCES customers (customer_id)
        ON DELETE RESTRICT,
    CONSTRAINT chk_orders_status CHECK (
        status IN ('Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled')
    )
);

CREATE INDEX IF NOT EXISTS ix_orders_status
    ON orders (status);

CREATE INDEX IF NOT EXISTS ix_orders_created
    ON orders (created);

CREATE INDEX IF NOT EXISTS ix_orders_kund_id
    ON orders (kund_id);

-- ---------------------------------------------------------------------------
-- order_lines
-- order_id  → orders   CASCADE : deleting an order removes its lines.
-- product_id → products RESTRICT : cannot delete a product used in an order.
-- namn / pris are snapshots captured at order time — never updated.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS order_lines (
    order_line_id  TEXT            NOT NULL,
    order_id       TEXT            NOT NULL,
    product_id     TEXT            NOT NULL,
    namn           TEXT            NOT NULL,
    pris           NUMERIC(18, 2)  NOT NULL CHECK (pris > 0),
    antal          INTEGER         NOT NULL CHECK (antal > 0),

    CONSTRAINT pk_order_lines PRIMARY KEY (order_line_id),
    CONSTRAINT fk_order_lines_order
        FOREIGN KEY (order_id)
        REFERENCES orders (order_id)
        ON DELETE CASCADE,
    CONSTRAINT fk_order_lines_product
        FOREIGN KEY (product_id)
        REFERENCES products (product_id)
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_order_lines_order_id
    ON order_lines (order_id);

CREATE INDEX IF NOT EXISTS ix_order_lines_product_id
    ON order_lines (product_id);