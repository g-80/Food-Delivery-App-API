CREATE TABLE carts (
    id integer primary key generated by default as identity,
    customer_id integer REFERENCES users(id) ON DELETE RESTRICT,
    is_used boolean DEFAULT FALSE,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    expires_at timestamp with time zone DEFAULT (CURRENT_TIMESTAMP + INTERVAL '5 minutes')
);


CREATE TABLE cart_items (
    id integer primary key generated by default as identity,
    cart_id integer NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    item_id integer NOT NULL REFERENCES food_places_items(id) ON DELETE CASCADE,
    quantity integer NOT NULL,
    unit_price integer NOT NULL,
    subtotal integer NOT NULL
);


CREATE TABLE cart_pricings (
    cart_id integer primary key REFERENCES carts(id) ON DELETE CASCADE,
    subtotal integer NOT NULL,
    fees integer NOT NULL,
    delivery_fee integer,
    total integer NOT NULL
);


CREATE OR REPLACE FUNCTION update_cart_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE carts
    SET updated_at = CURRENT_TIMESTAMP
    WHERE id = NEW.cart_id OR id = OLD.cart_id;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER cart_items_update_trigger
AFTER INSERT OR UPDATE OR DELETE
ON cart_items
FOR EACH ROW
EXECUTE FUNCTION update_cart_timestamp();
