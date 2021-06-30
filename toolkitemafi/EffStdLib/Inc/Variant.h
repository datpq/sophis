#pragma once
#include <stdlib.h>     /* malloc, calloc, realloc, free */
#include <assert.h>     /* assert */
#include <vector>
#include <iostream>

using namespace std;

namespace eff {
	namespace utils {
		class Variant
		{
		private:
			struct Head {
				virtual ~Head() {}
				virtual void *copy() = 0;
				const type_info& type;
				Head(const type_info& type): type(type) {}
				void *data() { return this + 1; }
			};
			template <class T> struct THead: public Head {
				THead(): Head(typeid(T)) {}
				//virtual ~THead() override { ((T*)data())->~T(); }
				virtual ~THead() { ((T*)data())->~T(); }
				virtual void *copy() override {
					return new(new(malloc(sizeof(Head) + sizeof(T)))
						THead() + 1) T(*(const T*)data()); }
			};
			void *data;
			Head *head() const { return (Head*)data - 1; }
			void *copy() const { return data ? head()->copy() : nullptr; }
		public:
			Variant(): data(nullptr) {}
			Variant(const Variant& src): data(src.copy()) {}
			Variant(Variant&& src): data(src.data) { src.data = nullptr; }
			template <class T> Variant(const T& src): data(
				new(new(malloc(sizeof(Head) + sizeof(T))) THead<T>() + 1) T(src)) {}
			~Variant() {
				if(!data) return;
				Head* head = this->head();
				head->~Head(); free(head); }
			bool empty() const {
				return data == nullptr; }
			const type_info& type() const {
				assert(data);
				return ((Head*)data - 1)->type; }
			template <class T>
			T& value() {
				if (!data || type() != typeid(T))
					throw bad_cast();
				return *(T*)data; }
			template <class T>
			const T& value() const {
				if (!data || type() != typeid(T))
					throw bad_cast();
				return *(T*)data; }
			template <class T>
			void setValue(const T& it) {
				if(!data)
					data = new(new(malloc(sizeof(Head) + sizeof(T)))
					THead<T>() + 1) T(it);
				else {
					if (type() != typeid(T)) throw bad_cast();
					*(T*)data = it; }}
		public:
			static void test_me() {
				vector<Variant> list;
				list.push_back(Variant(1));
				list.push_back(3.14);
				list.push_back(string("hello world"));
				list[1].value<double>() = 3.141592;
				list.push_back(Variant());
				list[3].setValue(1.23f);
				//for (auto& a : list) {
				for (auto it = list.begin() ; it != list.end(); ++it) {
					Variant& a = (*it);
					cout << "type = " << a.type().name() << " value = ";
					if(a.type() == typeid(int)) cout << a.value<int>();
					else if (a.type() == typeid(double)) cout << a.value<double>();
					else if (a.type() == typeid(string)) cout << a.value<string>();
					else if (a.type() == typeid(float))  cout << a.value<float>();
					cout << endl;
				}
			}
		};
	}
}