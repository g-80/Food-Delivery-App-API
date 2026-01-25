-- ============================================================================
-- Food Places Database Seed File
-- ============================================================================
-- This file seeds the food_places_locations, food_places, and food_places_items tables
-- All food places are assigned to user_id = 2
-- Items are matched to food places based on category
-- ============================================================================

BEGIN;

-- ============================================================================
-- Insert Food Place Locations
-- ============================================================================

INSERT INTO public.food_places_locations (location) VALUES
(ST_SetSRID(ST_MakePoint(-0.194214593361113, 51.5059707375735), 4326)),  -- 1: Pasta Paradiso
(ST_SetSRID(ST_MakePoint(-0.193823194712062, 51.5068824113275), 4326)),  -- 2: La Pergola
(ST_SetSRID(ST_MakePoint(-0.161420414170372, 51.5140815112849), 4326)),  -- 3: Venezia Vibes
(ST_SetSRID(ST_MakePoint(-0.103104000381302, 51.529246620244), 4326)),   -- 4: Bella Cucina
(ST_SetSRID(ST_MakePoint(-0.0836903095585595, 51.510643323096), 4326)),  -- 5: Il Forno Antico
(ST_SetSRID(ST_MakePoint(-0.271339921461815, 51.5071810828934), 4326)),  -- 6: Ravioli e Ricordi
(ST_SetSRID(ST_MakePoint(-0.103004641743105, 51.5359280778319), 4326)),  -- 7: Pizzeria Napoli
(ST_SetSRID(ST_MakePoint(-0.102895856463621, 51.5140817690418), 4326)),  -- 8: Sapori Toscani
(ST_SetSRID(ST_MakePoint(-0.102663863093786, 51.5395906168261), 4326)),  -- 9: Gusto Italiano
(ST_SetSRID(ST_MakePoint(-0.0938717502134456, 51.48470000742), 4326)),   -- 10: Ristorante Sapori di Nonna
(ST_SetSRID(ST_MakePoint(-0.101115733681461, 51.5129267593386), 4326)),  -- 11: Casa del Sabor
(ST_SetSRID(ST_MakePoint(-0.104606782704904, 51.5290997076809), 4326)),  -- 12: El Rincón
(ST_SetSRID(ST_MakePoint(-0.0717535973480299, 51.5252087688257), 4326)), -- 13: La Paellera
(ST_SetSRID(ST_MakePoint(-0.146806044869447, 51.5191627059317), 4326)),  -- 14: El Mesón Andaluz
(ST_SetSRID(ST_MakePoint(-0.1744685, 51.5208504), 4326)),                -- 15: Mariscos del Sol
(ST_SetSRID(ST_MakePoint(-0.10748353743911, 51.5242364502699), 4326)),   -- 16: Paella Place
(ST_SetSRID(ST_MakePoint(-0.192445612765786, 51.5098992637101), 4326)),  -- 17: Tierra de Tapas
(ST_SetSRID(ST_MakePoint(-0.162094071063321, 51.5145143266588), 4326)),  -- 18: Mar y Montaña
(ST_SetSRID(ST_MakePoint(-0.0810794628115619, 51.5253697717081), 4326)), -- 19: Sabor de España
(ST_SetSRID(ST_MakePoint(-0.192411977430309, 51.5387735086986), 4326)),  -- 20: El Toro
(ST_SetSRID(ST_MakePoint(-0.108016285662846, 51.518737673634), 4326)),   -- 21: Delhi Delight
(ST_SetSRID(ST_MakePoint(-0.116873320856872, 51.5413157517853), 4326)),  -- 22: Tandoori Mahal
(ST_SetSRID(ST_MakePoint(-0.206724192806853, 51.5067388172373), 4326)),  -- 23: Curry and Tastes
(ST_SetSRID(ST_MakePoint(-0.101036617111685, 51.539562471222), 4326)),   -- 24: Biryani Place
(ST_SetSRID(ST_MakePoint(-0.0741558797115638, 51.524572089102), 4326)),  -- 25: The Taj Mahal
(ST_SetSRID(ST_MakePoint(-0.109730554818275, 51.5334297507424), 4326)),  -- 26: Tastes of Punjab
(ST_SetSRID(ST_MakePoint(-0.286035232778949, 51.509838183819), 4326)),   -- 27: Shah's Kitchen
(ST_SetSRID(ST_MakePoint(-0.0965399250187837, 51.4992754129483), 4326)), -- 28: The Spice Bazaar
(ST_SetSRID(ST_MakePoint(-0.183658397558033, 51.4981503609359), 4326)),  -- 29: Tamil Land
(ST_SetSRID(ST_MakePoint(-0.141325083250043, 51.5190725410778), 4326)),  -- 30: Madhu
(ST_SetSRID(ST_MakePoint(-0.0813348207914633, 51.5266874051039), 4326)), -- 31: Caspian Palace
(ST_SetSRID(ST_MakePoint(-0.12400808429115, 51.5143750250394), 4326)),   -- 32: Roses of Persia
(ST_SetSRID(ST_MakePoint(-0.106052316178245, 51.534977367321), 4326)),   -- 33: Palace of Persia
(ST_SetSRID(ST_MakePoint(-0.11074351633594, 51.5000969646496), 4326)),   -- 34: Taste of Saffron
(ST_SetSRID(ST_MakePoint(-0.305834682739705, 51.5120295040342), 4326)),  -- 35: Persian Rice House
(ST_SetSRID(ST_MakePoint(-0.318990651454374, 51.5045085805595), 4326)),  -- 36: Tastes of Persia
(ST_SetSRID(ST_MakePoint(-0.235848334428606, 51.5065435238364), 4326)),  -- 37: Mumtaz Kitchen
(ST_SetSRID(ST_MakePoint(-0.195547158941114, 51.5001792070748), 4326)),  -- 38: Molani
(ST_SetSRID(ST_MakePoint(-0.302251973925545, 51.5145932121357), 4326)),  -- 39: Maryam's Cuisine
(ST_SetSRID(ST_MakePoint(-0.157068765847905, 51.541217212646), 4326)),   -- 40: Iran Restaurant
(ST_SetSRID(ST_MakePoint(-0.102945304317033, 51.5396598631046), 4326)),  -- 41: The Greek Garden
(ST_SetSRID(ST_MakePoint(-0.315125209781788, 51.5116301240048), 4326)),  -- 42: Kipos Ellados
(ST_SetSRID(ST_MakePoint(-0.104761652905667, 51.529568617251), 4326)),   -- 43: Galazia Thalassa
(ST_SetSRID(ST_MakePoint(-0.273691673532079, 51.5072333292121), 4326)),  -- 44: The Hellenic Flavours
(ST_SetSRID(ST_MakePoint(-0.108315079791046, 51.5328191172659), 4326)),  -- 45: The Olive Grove
(ST_SetSRID(ST_MakePoint(-0.0649001246372261, 51.5264854985224), 4326)), -- 46: Alexander's Kitchen
(ST_SetSRID(ST_MakePoint(-0.106050629614216, 51.5375459047639), 4326)),  -- 47: Tastes of Greece
(ST_SetSRID(ST_MakePoint(-0.196041439836068, 51.536936079475), 4326)),   -- 48: Gyros of Athens
(ST_SetSRID(ST_MakePoint(-0.316069656518425, 51.5115901467696), 4326)),  -- 49: The Greek Grill
(ST_SetSRID(ST_MakePoint(-0.129809189980288, 51.5116358440606), 4326)),  -- 50: Souvlaki Place
(ST_SetSRID(ST_MakePoint(-0.135765799655294, 51.5210356081663), 4326)),  -- 51: Wagababa
(ST_SetSRID(ST_MakePoint(-0.139661441299863, 51.5520497380348), 4326)),  -- 52: Hikari Ramen
(ST_SetSRID(ST_MakePoint(-0.106278089749443, 51.5141658809694), 4326)),  -- 53: Umi No Kaze
(ST_SetSRID(ST_MakePoint(-0.0769798527798726, 51.5161900322252), 4326)), -- 54: Sakura Restaurant
(ST_SetSRID(ST_MakePoint(-0.271318297870777, 51.5086286352862), 4326)),  -- 55: Kin Koi Shokudo
(ST_SetSRID(ST_MakePoint(-0.123505003560396, 51.5135714550108), 4326)),  -- 56: Kyoto Udon
(ST_SetSRID(ST_MakePoint(-0.235527897025374, 51.4929453727535), 4326)),  -- 57: Ikana
(ST_SetSRID(ST_MakePoint(-0.132589204184368, 51.5117972906981), 4326)),  -- 58: Sushi and Sticks
(ST_SetSRID(ST_MakePoint(-0.10255961104759, 51.5405076743387), 4326)),   -- 59: Iwasaki Restaurant
(ST_SetSRID(ST_MakePoint(-0.115163500144797, 51.5210137014301), 4326)),  -- 60: Mochizuki
(ST_SetSRID(ST_MakePoint(-0.163067131019772, 51.5151509281966), 4326)),  -- 61: Beirut Nights
(ST_SetSRID(ST_MakePoint(-0.256556374288799, 51.5062793030106), 4326)),  -- 62: Zeit & Zaatar
(ST_SetSRID(ST_MakePoint(-0.1539677, 51.5137194), 4326)),                -- 63: Taste of Lebanon
(ST_SetSRID(ST_MakePoint(-0.105236520059886, 51.534394234183), 4326)),   -- 64: Al-Jabal
(ST_SetSRID(ST_MakePoint(-0.249122337092629, 51.5059015178677), 4326)),  -- 65: The Olive House
(ST_SetSRID(ST_MakePoint(-0.203043585571543, 51.497761296365), 4326)),   -- 66: Yalla Shawarma
(ST_SetSRID(ST_MakePoint(-0.1396769, 51.513257), 4326)),                 -- 67: Layla's Kitchen
(ST_SetSRID(ST_MakePoint(-0.1607749, 51.515838), 4326)),                 -- 68: Arzet Libnan Restaurant
(ST_SetSRID(ST_MakePoint(-0.249179896778746, 51.5146984820891), 4326)),  -- 69: Shawarma Palace
(ST_SetSRID(ST_MakePoint(-0.145475628648897, 51.5404318811101), 4326)),  -- 70: Beirut Nights
(ST_SetSRID(ST_MakePoint(-0.0708932046700461, 51.525183131582), 4326)),  -- 71: The Wok
(ST_SetSRID(ST_MakePoint(-0.119336620362784, 51.5160790218067), 4326)),  -- 72: The Golden Dragon
(ST_SetSRID(ST_MakePoint(-0.127115415870094, 51.5133709179573), 4326)),  -- 73: Yen's Kitchen
(ST_SetSRID(ST_MakePoint(-0.097090678278382, 51.5397316257271), 4326)),  -- 74: Taste Of China
(ST_SetSRID(ST_MakePoint(-0.0835716347065141, 51.511995344151), 4326)),  -- 75: Lotus Restaurant
(ST_SetSRID(ST_MakePoint(-0.1534229, 51.515584), 4326)),                 -- 76: Noodles and Sticks
(ST_SetSRID(ST_MakePoint(-0.204786274638216, 51.5158189097993), 4326)),  -- 77: Red Dragon Restaurant
(ST_SetSRID(ST_MakePoint(-0.3178835001479, 51.5040418130528), 4326)),    -- 78: Xi's Noodles and Rice
(ST_SetSRID(ST_MakePoint(-0.221961642129835, 51.500433749479), 4326)),   -- 79: The Orient
(ST_SetSRID(ST_MakePoint(-0.159582355402963, 51.5233100503163), 4326)),  -- 80: The Dynasty
(ST_SetSRID(ST_MakePoint(-0.13562754997487, 51.5188758124007), 4326)),   -- 81: Flying Pizza
(ST_SetSRID(ST_MakePoint(-0.0850254016450647, 51.5106915372124), 4326)), -- 82: Super Pizza
(ST_SetSRID(ST_MakePoint(-0.165749934508475, 51.5002068407911), 4326)),  -- 83: Who's Pizza
(ST_SetSRID(ST_MakePoint(-0.10565504677577, 51.5142181933442), 4326)),   -- 84: The Well Stacked Pizza Co.
(ST_SetSRID(ST_MakePoint(-0.222961946076969, 51.4889285807712), 4326)),  -- 85: Pizza Napoletana
(ST_SetSRID(ST_MakePoint(-0.125257865354049, 51.5132851016372), 4326)),  -- 86: Freddy Fazbear's Pizza
(ST_SetSRID(ST_MakePoint(-0.135368429456328, 51.5207202577906), 4326)),  -- 87: Epic Pizza
(ST_SetSRID(ST_MakePoint(-0.140560412559956, 51.5098622764886), 4326)),  -- 88: Mario Pizza
(ST_SetSRID(ST_MakePoint(-0.110600436215449, 51.5333932950116), 4326)),  -- 89: Pizza Planet
(ST_SetSRID(ST_MakePoint(-0.0761105418868024, 51.5412450262381), 4326)), -- 90: Pizza This
(ST_SetSRID(ST_MakePoint(-0.315645641124489, 51.5116829951949), 4326)),  -- 91: Burger Shot
(ST_SetSRID(ST_MakePoint(-0.201298021934468, 51.5130680796805), 4326)),  -- 92: Cluckin' Bell
(ST_SetSRID(ST_MakePoint(-0.194871923075393, 51.5077220512516), 4326)),  -- 93: Los Pollos Hermanos
(ST_SetSRID(ST_MakePoint(-0.198570279567287, 51.5146149838965), 4326));  -- 94: Dinner Bell

-- ============================================================================
-- Insert Food Places
-- ============================================================================

INSERT INTO public.food_places (name, category, description, location_id, address_id, user_id) VALUES
-- Italian Restaurants (1-10)
('Pasta Paradiso', 'Italian', NULL, 1, NULL, 2),
('La Pergola', 'Italian', NULL, 2, NULL, 2),
('Venezia Vibes', 'Italian', NULL, 3, NULL, 2),
('Bella Cucina', 'Italian', NULL, 4, NULL, 2),
('Il Forno Antico', 'Italian', NULL, 5, NULL, 2),
('Ravioli e Ricordi', 'Italian', NULL, 6, NULL, 2),
('Pizzeria Napoli', 'Italian', NULL, 7, NULL, 2),
('Sapori Toscani', 'Italian', NULL, 8, NULL, 2),
('Gusto Italiano', 'Italian', NULL, 9, NULL, 2),
('Ristorante Sapori di Nonna', 'Italian', NULL, 10, NULL, 2),

-- Spanish Restaurants (11-20)
('Casa del Sabor', 'Spanish', NULL, 11, NULL, 2),
('El Rincón', 'Spanish', NULL, 12, NULL, 2),
('La Paellera', 'Spanish', NULL, 13, NULL, 2),
('El Mesón Andaluz', 'Spanish', NULL, 14, NULL, 2),
('Mariscos del Sol', 'Spanish', NULL, 15, NULL, 2),
('Paella Place', 'Spanish', NULL, 16, NULL, 2),
('Tierra de Tapas', 'Spanish', NULL, 17, NULL, 2),
('Mar y Montaña', 'Spanish', NULL, 18, NULL, 2),
('Sabor de España', 'Spanish', NULL, 19, NULL, 2),
('El Toro', 'Spanish', NULL, 20, NULL, 2),

-- Indian Restaurants (21-30)
('Delhi Delight', 'Indian', NULL, 21, NULL, 2),
('Tandoori Mahal', 'Indian', NULL, 22, NULL, 2),
('Curry and Tastes', 'Indian', NULL, 23, NULL, 2),
('Biryani Place', 'Indian', NULL, 24, NULL, 2),
('The Taj Mahal', 'Indian', NULL, 25, NULL, 2),
('Tastes of Punjab', 'Indian', NULL, 26, NULL, 2),
('Shah''s Kitchen', 'Indian', NULL, 27, NULL, 2),
('The Spice Bazaar', 'Indian', NULL, 28, NULL, 2),
('Tamil Land', 'Indian', NULL, 29, NULL, 2),
('Madhu', 'Indian', NULL, 30, NULL, 2),

-- Persian Restaurants (31-40)
('Caspian Palace', 'Persian', NULL, 31, NULL, 2),
('Roses of Persia', 'Persian', NULL, 32, NULL, 2),
('Palace of Persia', 'Persian', NULL, 33, NULL, 2),
('Taste of Saffron', 'Persian', NULL, 34, NULL, 2),
('Persian Rice House', 'Persian', NULL, 35, NULL, 2),
('Tastes of Persia', 'Persian', NULL, 36, NULL, 2),
('Mumtaz Kitchen', 'Persian', NULL, 37, NULL, 2),
('Molani', 'Persian', NULL, 38, NULL, 2),
('Maryam''s Cuisine', 'Persian', NULL, 39, NULL, 2),
('Iran Restaurant', 'Persian', NULL, 40, NULL, 2),

-- Greek Restaurants (41-50)
('The Greek Garden', 'Greek', NULL, 41, NULL, 2),
('Kipos Ellados', 'Greek', NULL, 42, NULL, 2),
('Galazia Thalassa', 'Greek', NULL, 43, NULL, 2),
('The Hellenic Flavours', 'Greek', NULL, 44, NULL, 2),
('The Olive Grove', 'Greek', NULL, 45, NULL, 2),
('Alexander''s Kitchen', 'Greek', NULL, 46, NULL, 2),
('Tastes of Greece', 'Greek', NULL, 47, NULL, 2),
('Gyros of Athens', 'Greek', NULL, 48, NULL, 2),
('The Greek Grill', 'Greek', NULL, 49, NULL, 2),
('Souvlaki Place', 'Greek', NULL, 50, NULL, 2),

-- Japanese Restaurants (51-60)
('Wagababa', 'Japanese', NULL, 51, NULL, 2),
('Hikari Ramen', 'Japanese', NULL, 52, NULL, 2),
('Umi No Kaze', 'Japanese', NULL, 53, NULL, 2),
('Sakura Restaurant', 'Japanese', NULL, 54, NULL, 2),
('Kin Koi Shokudo', 'Japanese', NULL, 55, NULL, 2),
('Kyoto Udon', 'Japanese', NULL, 56, NULL, 2),
('Ikana', 'Japanese', NULL, 57, NULL, 2),
('Sushi and Sticks', 'Japanese', NULL, 58, NULL, 2),
('Iwasaki Restaurant', 'Japanese', NULL, 59, NULL, 2),
('Mochizuki', 'Japanese', NULL, 60, NULL, 2),

-- Lebanese Restaurants (61-70)
('Beirut Nights', 'Lebanese', NULL, 61, NULL, 2),
('Zeit & Zaatar', 'Lebanese', NULL, 62, NULL, 2),
('Taste of Lebanon', 'Lebanese', NULL, 63, NULL, 2),
('Al-Jabal', 'Lebanese', NULL, 64, NULL, 2),
('The Olive House', 'Lebanese', NULL, 65, NULL, 2),
('Yalla Shawarma', 'Lebanese', NULL, 66, NULL, 2),
('Layla''s Kitchen', 'Lebanese', NULL, 67, NULL, 2),
('Arzet Libnan Restaurant', 'Lebanese', NULL, 68, NULL, 2),
('Shawarma Palace', 'Lebanese', NULL, 69, NULL, 2),
('Beirut Nights', 'Lebanese', NULL, 70, NULL, 2),

-- Chinese Restaurants (71-80)
('The Wok', 'Chinese', NULL, 71, NULL, 2),
('The Golden Dragon', 'Chinese', NULL, 72, NULL, 2),
('Yen''s Kitchen', 'Chinese', NULL, 73, NULL, 2),
('Taste Of China', 'Chinese', NULL, 74, NULL, 2),
('Lotus Restaurant', 'Chinese', NULL, 75, NULL, 2),
('Noodles and Sticks', 'Chinese', NULL, 76, NULL, 2),
('Red Dragon Restaurant', 'Chinese', NULL, 77, NULL, 2),
('Xi''s Noodles and Rice', 'Chinese', NULL, 78, NULL, 2),
('The Orient', 'Chinese', NULL, 79, NULL, 2),
('The Dynasty', 'Chinese', NULL, 80, NULL, 2),

-- Pizza Restaurants (81-90)
('Flying Pizza', 'Pizza', NULL, 81, NULL, 2),
('Super Pizza', 'Pizza', NULL, 82, NULL, 2),
('Who''s Pizza', 'Pizza', NULL, 83, NULL, 2),
('The Well Stacked Pizza Co.', 'Pizza', NULL, 84, NULL, 2),
('Pizza Napoletana', 'Pizza', NULL, 85, NULL, 2),
('Freddy Fazbear''s Pizza', 'Pizza', NULL, 86, NULL, 2),
('Epic Pizza', 'Pizza', NULL, 87, NULL, 2),
('Mario Pizza', 'Pizza', NULL, 88, NULL, 2),
('Pizza Planet', 'Pizza', NULL, 89, NULL, 2),
('Pizza This', 'Pizza', NULL, 90, NULL, 2),

-- Fast Food Restaurants (91-94)
('Burger Shot', 'Fast food', NULL, 91, NULL, 2),
('Cluckin'' Bell', 'Fast food', NULL, 92, NULL, 2),
('Los Pollos Hermanos', 'Fast food', NULL, 93, NULL, 2),
('Dinner Bell', 'Fast food', NULL, 94, NULL, 2);

-- ============================================================================
-- Insert Food Items for Each Category
-- ============================================================================

-- Italian Items (for food_place_id 1-10)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Spaghetti Carbonara', 'Classic Roman pasta with guanciale, pecorino romano, eggs and black pepper', 1450, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Margherita Pizza', 'Traditional Neapolitan pizza with San Marzano tomatoes, mozzarella and basil', 1250, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Risotto ai Funghi', 'Creamy arborio rice with mixed wild mushrooms and parmesan', 1650, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Osso Buco', 'Braised veal shanks in white wine with gremolata and saffron risotto', 2850, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Tiramisu', 'Classic Italian dessert with espresso-soaked ladyfingers and mascarpone', 650, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Penne Arrabbiata', 'Penne pasta in spicy tomato sauce with garlic and chilli', 1150, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Saltimbocca alla Romana', 'Veal escalopes with prosciutto and sage in white wine sauce', 2250, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Caprese Salad', 'Fresh buffalo mozzarella, tomatoes, basil and extra virgin olive oil', 950, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Lasagne al Forno', 'Layered pasta with beef ragu, bechamel and parmesan', 1550, true
FROM public.food_places fp WHERE fp.category = 'Italian'
UNION ALL
SELECT fp.id, 'Panna Cotta', 'Silky vanilla cream dessert with berry compote', 600, true
FROM public.food_places fp WHERE fp.category = 'Italian';

-- Spanish Items (for food_place_id 11-20)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Paella Valenciana', 'Saffron rice with chicken, rabbit, green beans and butter beans', 1850, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Patatas Bravas', 'Crispy fried potatoes with spicy tomato sauce and aioli', 750, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Gambas al Ajillo', 'Sizzling prawns in garlic, chilli and olive oil', 1450, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Tortilla Española', 'Traditional Spanish omelette with potatoes and onions', 850, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Jamón Ibérico', 'Hand-carved acorn-fed Iberian ham with breadsticks', 1950, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Pulpo a la Gallega', 'Galician-style octopus with paprika, olive oil and potatoes', 1750, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Croquetas de Jamón', 'Creamy ham croquettes with bechamel filling', 950, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Gazpacho', 'Chilled Andalusian tomato soup with cucumber and peppers', 650, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Churros con Chocolate', 'Fried dough pastries with thick hot chocolate dipping sauce', 550, true
FROM public.food_places fp WHERE fp.category = 'Spanish'
UNION ALL
SELECT fp.id, 'Albondigas', 'Spanish meatballs in rich tomato sauce', 1250, true
FROM public.food_places fp WHERE fp.category = 'Spanish';

-- Indian Items (for food_place_id 21-30)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Chicken Tikka Masala', 'Tandoori chicken in creamy tomato and fenugreek sauce', 1350, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Lamb Rogan Josh', 'Tender lamb curry with aromatic Kashmiri spices', 1550, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Palak Paneer', 'Indian cottage cheese in spiced spinach gravy', 1150, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Butter Chicken', 'Marinated chicken in rich tomato and butter sauce', 1350, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Vegetable Biryani', 'Fragrant basmati rice with mixed vegetables and whole spices', 1050, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Samosa Chaat', 'Crispy samosas topped with chickpeas, yogurt and tamarind chutney', 750, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Tandoori Mixed Grill', 'Assorted marinated meats cooked in clay oven', 1850, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Aloo Gobi', 'Cauliflower and potato curry with cumin and turmeric', 950, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Gulab Jamun', 'Sweet milk dumplings in rose-scented syrup', 450, true
FROM public.food_places fp WHERE fp.category = 'Indian'
UNION ALL
SELECT fp.id, 'Chicken Vindaloo', 'Fiery Goan curry with chicken and vinegar-based sauce', 1450, true
FROM public.food_places fp WHERE fp.category = 'Indian';

-- Persian Items (for food_place_id 31-40)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Chelo Kebab Koobideh', 'Grilled minced lamb skewers with saffron rice and grilled tomato', 1650, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Ghormeh Sabzi', 'Herb stew with lamb, kidney beans and dried lime', 1550, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Zereshk Polo ba Morgh', 'Saffron chicken with barberry rice and pistachios', 1750, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Fesenjan', 'Chicken in rich walnut and pomegranate sauce', 1650, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Kashke Bademjan', 'Smoky aubergine dip with whey, caramelized onions and walnuts', 850, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Tahdig', 'Crispy saffron rice with yogurt crust', 950, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Mirza Ghasemi', 'Smoked aubergine with tomato, garlic and egg', 750, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Joojeh Kebab', 'Saffron and lemon marinated chicken skewers', 1450, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Mast-o-Khiar', 'Yogurt dip with cucumber, mint and rose petals', 550, true
FROM public.food_places fp WHERE fp.category = 'Persian'
UNION ALL
SELECT fp.id, 'Baklava', 'Layers of filo pastry with honey, pistachios and cardamom', 600, true
FROM public.food_places fp WHERE fp.category = 'Persian';

-- Greek Items (for food_place_id 41-50)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Moussaka', 'Layered aubergine, potato and lamb with bechamel topping', 1550, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Souvlaki', 'Grilled pork skewers with tzatziki and pita bread', 1250, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Greek Salad', 'Tomatoes, cucumber, olives, feta and oregano', 850, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Spanakopita', 'Spinach and feta cheese in crispy filo pastry', 950, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Dolmades', 'Vine leaves stuffed with rice, herbs and pine nuts', 750, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Kleftiko', 'Slow-roasted lamb with lemon, garlic and oregano', 1950, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Saganaki', 'Fried Kefalograviera cheese flambéed with ouzo', 850, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Gigantes Plaki', 'Giant butter beans in tomato sauce with herbs', 750, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Baklava', 'Honey-soaked filo pastry with walnuts and cinnamon', 550, true
FROM public.food_places fp WHERE fp.category = 'Greek'
UNION ALL
SELECT fp.id, 'Tzatziki with Pita', 'Yogurt dip with cucumber, garlic and fresh pita', 650, true
FROM public.food_places fp WHERE fp.category = 'Greek';

-- Japanese Items (for food_place_id 51-60)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Chicken Katsu Curry', 'Breaded chicken cutlet with Japanese curry sauce and rice', 1350, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Salmon Nigiri Set', 'Six pieces of fresh salmon sushi on seasoned rice', 1650, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Ramen', 'Rich pork bone broth with noodles, chashu pork and soft boiled egg', 1450, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Vegetable Tempura', 'Lightly battered and fried seasonal vegetables', 1050, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Teriyaki Chicken Donburi', 'Glazed chicken with vegetables over steamed rice', 1250, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'California Roll', 'Inside-out sushi with crab, avocado and cucumber', 950, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Gyoza', 'Pan-fried pork and vegetable dumplings with ponzu sauce', 850, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Miso Soup', 'Traditional soybean paste soup with tofu and wakame', 450, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Edamame', 'Steamed young soybeans with sea salt', 550, true
FROM public.food_places fp WHERE fp.category = 'Japanese'
UNION ALL
SELECT fp.id, 'Matcha Ice Cream', 'Green tea flavoured Japanese ice cream', 500, true
FROM public.food_places fp WHERE fp.category = 'Japanese';

-- Lebanese Items (for food_place_id 61-70)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Mixed Mezze Platter', 'Hummus, baba ganoush, tabbouleh, falafel and pita', 1450, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Lamb Shawarma', 'Marinated lamb slices with tahini, pickles and flatbread', 1350, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Chicken Tawook', 'Grilled garlic and lemon marinated chicken skewers', 1250, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Falafel Wrap', 'Crispy chickpea fritters with tahini and salad in flatbread', 850, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Fattoush Salad', 'Mixed greens with crispy pita chips and sumac dressing', 750, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Kibbeh', 'Bulgur wheat shells stuffed with spiced lamb and pine nuts', 1150, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Manakish Zaatar', 'Flatbread topped with thyme, sesame and olive oil', 650, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Batata Harra', 'Spicy Lebanese potatoes with coriander and chilli', 750, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Baklava', 'Sweet pastry with pistachios and orange blossom syrup', 550, true
FROM public.food_places fp WHERE fp.category = 'Lebanese'
UNION ALL
SELECT fp.id, 'Labneh with Olive Oil', 'Thick strained yogurt drizzled with olive oil and herbs', 650, true
FROM public.food_places fp WHERE fp.category = 'Lebanese';

-- Chinese Items (for food_place_id 71-80)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Sweet and Sour Chicken', 'Crispy chicken in tangy pineapple sauce with peppers', 1250, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Kung Pao Chicken', 'Stir-fried chicken with peanuts, vegetables and chilli', 1350, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Beef in Black Bean Sauce', 'Tender beef strips with peppers in fermented black bean sauce', 1450, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Singapore Noodles', 'Curry-spiced rice noodles with prawns, pork and vegetables', 1250, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Crispy Aromatic Duck', 'Shredded duck with pancakes, cucumber and hoisin sauce', 1850, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Dim Sum Platter', 'Assorted steamed dumplings including har gow and siu mai', 1550, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Ma Po Tofu', 'Silken tofu in spicy Sichuan sauce with minced pork', 1150, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Egg Fried Rice', 'Wok-fried rice with egg, peas and spring onions', 750, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Spring Rolls', 'Crispy vegetable rolls with sweet chilli dipping sauce', 650, true
FROM public.food_places fp WHERE fp.category = 'Chinese'
UNION ALL
SELECT fp.id, 'Sesame Prawn Toast', 'Deep-fried bread topped with minced prawns and sesame seeds', 850, true
FROM public.food_places fp WHERE fp.category = 'Chinese';

-- Pizza Items (for food_place_id 81-90)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Pepperoni Feast', 'Tomato sauce, mozzarella and double pepperoni', 1450, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'BBQ Chicken', 'BBQ sauce, chicken, red onions, peppers and mozzarella', 1550, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Quattro Formaggi', 'Four cheese pizza with mozzarella, gorgonzola, parmesan and goats cheese', 1650, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Vegetariana', 'Tomato sauce, mozzarella, mushrooms, peppers, onions and sweetcorn', 1350, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Hawaiian', 'Tomato sauce, mozzarella, ham and pineapple', 1250, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Meat Feast', 'Tomato sauce, mozzarella, pepperoni, ham, chicken and beef', 1750, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Margherita', 'Classic tomato sauce, mozzarella and basil', 1050, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Seafood Special', 'Tomato sauce, mozzarella, prawns, tuna, anchovies and mussels', 1850, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Diavola', 'Spicy tomato sauce, mozzarella, pepperoni and chilli flakes', 1450, true
FROM public.food_places fp WHERE fp.category = 'Pizza'
UNION ALL
SELECT fp.id, 'Garlic Bread with Cheese', 'Stone-baked garlic bread topped with melted mozzarella', 650, true
FROM public.food_places fp WHERE fp.category = 'Pizza';

-- Fast Food Items (for food_place_id 91-94)
INSERT INTO public.food_places_items (food_place_id, name, description, price, is_available) 
SELECT fp.id, 'Classic Beef Burger', 'Quarter pound beef patty with lettuce, tomato and burger sauce', 950, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Chicken Nuggets', 'Ten pieces of breaded chicken with choice of dipping sauce', 750, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Fish and Chips', 'Battered cod fillet with chunky chips and mushy peas', 1250, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Loaded Cheese Fries', 'Crispy fries topped with melted cheese, bacon and sour cream', 850, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Fried Chicken Meal', 'Three pieces of spicy fried chicken with coleslaw and fries', 1150, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Double Cheeseburger', 'Two beef patties with cheese, pickles and special sauce', 1250, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Chicken Wrap', 'Grilled chicken strips with lettuce, mayo and tortilla wrap', 850, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Onion Rings', 'Beer-battered crispy onion rings with ranch dip', 550, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Chicken Wings', 'Eight buffalo-style wings with blue cheese dressing', 950, true
FROM public.food_places fp WHERE fp.category = 'Fast food'
UNION ALL
SELECT fp.id, 'Vanilla Milkshake', 'Thick and creamy vanilla ice cream milkshake', 450, true
FROM public.food_places fp WHERE fp.category = 'Fast food';

COMMIT;