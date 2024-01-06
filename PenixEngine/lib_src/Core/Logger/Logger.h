#pragma once

#include <iostream>
namespace CoreLogger {
	template <typename T>
	void log(T log_message, bool newLine = true, uint16_t indentation = 0)
	{
		for (int i = 0; i < indentation; i++) {
			std::cout << "\t";
		}

		std::cout << log_message;

		if (newLine) {
			std::cout << std::endl;
		}
	}
}