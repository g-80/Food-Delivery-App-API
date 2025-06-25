WITH place_id AS (
    SELECT id
    FROM food_places
    WHERE name = 'Flying Pizza'
)
INSERT into food_places_items(name, description, food_place_id, is_available, price)
VALUES 
('Epic Pizza Special', 'Our signature pizza', (SELECT id from place_id), TRUE, 600),
('Margherita', NULL, (SELECT id from place_id), TRUE, 450),
('Chicken BBQ', NULL, (SELECT id from place_id), FALSE, 520);