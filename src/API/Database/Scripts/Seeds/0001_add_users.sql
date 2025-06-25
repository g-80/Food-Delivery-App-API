INSERT INTO users (id, first_name, surname, phone_number, password, user_type)
VALUES
(1, 'The', 'Customer', '07123456789', 'AQAAAAIAAYagAAAAENScaPI5e3xMYXOf6EbceTrzDGEtGog1all44Mohh39uBANH2Mfe5iLtYZjDPulKiQ==', 1),
(2, 'The', 'Food place', '07111222333', 'AQAAAAIAAYagAAAAENScaPI5e3xMYXOf6EbceTrzDGEtGog1all44Mohh39uBANH2Mfe5iLtYZjDPulKiQ==', 2),
(3, 'The', 'Driver', '07123123123', 'AQAAAAIAAYagAAAAENScaPI5e3xMYXOf6EbceTrzDGEtGog1all44Mohh39uBANH2Mfe5iLtYZjDPulKiQ==', 3),
(4, 'The', 'OtherDriver', '07456456456', 'AQAAAAIAAYagAAAAENScaPI5e3xMYXOf6EbceTrzDGEtGog1all44Mohh39uBANH2Mfe5iLtYZjDPulKiQ==', 3);

INSERT INTO carts (customer_id)
VALUES
(1);

INSERT INTO cart_pricings (cart_id, subtotal, fees, delivery_fee, total)
VALUES
(1, 0, 0, 0, 0);